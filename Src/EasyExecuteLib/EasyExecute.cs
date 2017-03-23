using Akka.Actor;
using EasyExecute.ActorSystemFactory;
using EasyExecute.Common;
using EasyExecute.Messages;
using EasyExecute.Reception;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyExecute.ExecutionQuery;

namespace EasyExecuteLib
{
    public class EasyExecute
    {
        private EasyExecuteMain _easyExecuteMain;
        private ActorSystemCreator ActorSystemCreator { get; set; }
        internal TimeSpan DefaultMaxExecutionTimePerAskCall = TimeSpan.FromSeconds(5);
        internal IActorRef ReceptionActorRef { get; set; }
        internal const bool DefaultReturnExistingResultWhenDuplicateId = true;

        //internal readonly int CacheExpirationNextHours = 1;
        internal TimeSpan DefaultPurgeInterval = TimeSpan.FromSeconds(30);

        #region Constructors

        public EasyExecute(
            TimeSpan? maxExecutionTimePerAskCall
          , string serverActorSystemName
          , ActorSystem actorSystem
          , string actorSystemConfig = null
          , TimeSpan? purgeInterval = null
          , Action<Worker> onWorkerPurged = null)
        {
            InitializeEasyExecute(
            maxExecutionTimePerAskCall
          , serverActorSystemName
          , actorSystem
          , actorSystemConfig
          , purgeInterval
          , onWorkerPurged);
        }

        public EasyExecute(
           TimeSpan? maxExecutionTimePerAskCall
         , ActorSystem actorSystem
         , string actorSystemConfig
         , TimeSpan? purgeInterval = null
         , Action<Worker> onWorkerPurged = null)
        {
            InitializeEasyExecute(
            maxExecutionTimePerAskCall
          , null
          , actorSystem
          , actorSystemConfig
          , purgeInterval
          , onWorkerPurged);
        }

        public EasyExecute(
          ActorSystem actorSystem
        , string actorSystemConfig
        , TimeSpan? purgeInterval = null
        , Action<Worker> onWorkerPurged = null)
        {
            InitializeEasyExecute(
            null
          , null
          , actorSystem
          , actorSystemConfig
          , purgeInterval
          , onWorkerPurged);
        }

        public EasyExecute(
        TimeSpan? purgeInterval = null
      , Action<Worker> onWorkerPurged = null)
        {
            InitializeEasyExecute(
            null
          , null
          , null
          , null
          , purgeInterval
          , onWorkerPurged);
        }

        private void InitializeEasyExecute(
            TimeSpan? maxExecutionTimePerAskCall = null
          , string serverActorSystemName = null
          , ActorSystem actorSystem = null
          , string actorSystemConfig = null
          , TimeSpan? purgeInterval = null
          , Action<Worker> onWorkerPurged = null)
        {
            serverActorSystemName = (string.IsNullOrEmpty(serverActorSystemName) && actorSystem == null)
                ? Guid.NewGuid().ToString()
                : serverActorSystemName;
            _easyExecuteMain = new EasyExecuteMain(this);
            ActorSystemCreator = new ActorSystemCreator();
            ActorSystemCreator.CreateOrSetUpActorSystem(serverActorSystemName, actorSystem, actorSystemConfig);
            ExecutionQueryActorRef = ActorSystemCreator.ServiceActorSystem.ActorOf(Props.Create(() => new ExecutionQueryActor()));
            ReceptionActorRef = ActorSystemCreator.ServiceActorSystem.ActorOf(Props.Create(() => new ReceptionActor(purgeInterval ?? DefaultPurgeInterval, onWorkerPurged, ExecutionQueryActorRef)));
            DefaultMaxExecutionTimePerAskCall = maxExecutionTimePerAskCall ?? DefaultMaxExecutionTimePerAskCall;
        }

        public IActorRef ExecutionQueryActorRef { get; set; }

        #endregion Constructors

        #region HAS ID NO COMMAND HAS RESULT

        public Task<ExecutionResult<TResult>> ExecuteAsync<TResult>(
         string id
       , Func<Task<TResult>> operation
       , TimeSpan? maxExecutionTimePerAskCall = null
       , ExecutionRequestOptions executionOptions = null
       , Func<ExecutionResult<TResult>, TResult> transformResult = null)
         where TResult : class
        {
            return _easyExecuteMain.Execute(
                id
              , new object()
              , (o) => operation()
              , null
              , maxExecutionTimePerAskCall
              , transformResult
              , executionOptions);
        }

        public Task<ExecutionResult<TResult>> ExecuteAsync<TResult>(
           string id
         , Func<Task<TResult>> operation
         , Func<TResult, bool> hasFailed
         , TimeSpan? maxExecutionTimePerAskCall = null
         , ExecutionRequestOptions executionOptions = null
         , Func<ExecutionResult<TResult>, TResult> transformResult = null)
           where TResult : class
        {
            return _easyExecuteMain.Execute(
                id
              , new object()
              , (o) => operation()
              , hasFailed
              , maxExecutionTimePerAskCall
              , transformResult
              , executionOptions);
        }

        #endregion HAS ID NO COMMAND HAS RESULT

        #region HAS ID  HAS COMMAND HAS RESULT

        public Task<ExecutionResult<TResult>> ExecuteAsync<TResult, TCommand>(
        string id
      , TCommand command
      , Func<TCommand, Task<TResult>> operation
      , TimeSpan? maxExecutionTimePerAskCall = null
      , ExecutionRequestOptions executionOptions = null
      , Func<ExecutionResult<TResult>, TResult> transformResult = null) where TResult : class
        {
            return _easyExecuteMain.Execute(
                id
              , command
              , operation
              , null
              , maxExecutionTimePerAskCall
              , transformResult
              , executionOptions);
        }

        public Task<ExecutionResult<TResult>> ExecuteAsync<TResult, TCommand>(
         string id
       , TCommand command
       , Func<TCommand, Task<TResult>> operation
       , Func<TResult, bool> hasFailed
       , TimeSpan? maxExecutionTimePerAskCall = null
       , ExecutionRequestOptions executionOptions = null
       , Func<ExecutionResult<TResult>, TResult> transformResult = null) where TResult : class
        {
            return _easyExecuteMain.Execute(
                id
              , command
              , operation
              , hasFailed
              , maxExecutionTimePerAskCall
              , transformResult
              , executionOptions);
        }

        #endregion HAS ID  HAS COMMAND HAS RESULT

        #region HAS ID   HAS COMMAND  NO RESULT

        public async Task<ExecutionResult> ExecuteAsync<TCommand>(
        string id
      , TCommand command
      , Action<TCommand> operation
      , TimeSpan? maxExecutionTimePerAskCall = null
      , ExecutionRequestOptions executionOptions = null)
        {
            var result = await _easyExecuteMain.Execute<object, TCommand>(
                id
              , command
              , (o) => { operation(o); return Task.FromResult(new object()); }
              , null

              , maxExecutionTimePerAskCall
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded,
                WorkerId = result.WorkerId
            };
        }

        public async Task<ExecutionResult> ExecuteAsync<TCommand>(
         string id
       , TCommand command
       , Action<TCommand> operation
       , Func<bool> hasFailed
       , TimeSpan? maxExecutionTimePerAskCall = null
       , ExecutionRequestOptions executionOptions = null)
        {
            var result = await _easyExecuteMain.Execute<object, TCommand>(
                id
              , command
              , (o) => { operation(o); return Task.FromResult(new object()); }
              , (r) => hasFailed()
              , maxExecutionTimePerAskCall
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded,
                WorkerId = result.WorkerId
            };
        }

        #endregion HAS ID   HAS COMMAND  NO RESULT

        #region HAS ID  NO COMMAND  NO RESULT

        public async Task<ExecutionResult> ExecuteAsync(
         string id
       , Action operation
       , TimeSpan? maxExecutionTimePerAskCall
       , ExecutionRequestOptions executionOptions = null)
        {
            var result = await _easyExecuteMain.Execute<object, object>(
                id
              , new object()
              , (o) => { operation(); return Task.FromResult(new object()); }
              , null
              , maxExecutionTimePerAskCall
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded,
                WorkerId = result.WorkerId
            };
        }

        public async Task<ExecutionResult> ExecuteAsync(
        string id
      , Action operation
      , Func<bool> hasFailed
      , TimeSpan? maxExecutionTimePerAskCall
      , ExecutionRequestOptions executionOptions = null)
        {
            var result = await _easyExecuteMain.Execute<object, object>(
                id
              , new object()
              , (o) => { operation(); return Task.FromResult(new object()); }
              , (r) => hasFailed()
              , maxExecutionTimePerAskCall
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded,
                WorkerId = result.WorkerId
            };
        }

        #endregion HAS ID  NO COMMAND  NO RESULT

        #region NO ID NO COMMAND  NO RESULT

        public async Task<ExecutionResult> ExecuteAsync(
         Action operation
       , TimeSpan? maxExecutionTimePerAskCall
       , ExecutionRequestOptions executionOptions = null)
        {
            var result = await _easyExecuteMain.Execute<object, object>(
                Guid.NewGuid().ToString()
              , new object()
              , (o) => { operation(); return Task.FromResult(new object()); }
              , null
              , maxExecutionTimePerAskCall
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded,
                WorkerId = result.WorkerId
            };
        }

        public async Task<ExecutionResult> ExecuteAsync(
        Action operation
      , ExecutionRequestOptions executionOptions = null)
        {
            var result = await _easyExecuteMain.Execute<object, object>(
                Guid.NewGuid().ToString()
              , new object()
              , (o) => { operation(); return Task.FromResult(new object()); }
              , null
              , null
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded,
                WorkerId = result.WorkerId
            };
        }

        public async Task<ExecutionResult> ExecuteAsync(
        Action operation
      , Func<bool> hasFailed
      , TimeSpan? maxExecutionTimePerAskCall
      , ExecutionRequestOptions executionOptions = null)
        {
            var result = await _easyExecuteMain.Execute<object, object>(
                Guid.NewGuid().ToString()
              , new object()
              , (o) => { operation(); return Task.FromResult(new object()); }
              , (r) => hasFailed()
              , maxExecutionTimePerAskCall
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded,
                WorkerId = result.WorkerId
            };
        }

        public async Task<ExecutionResult> ExecuteAsync(
       Action operation
     , Func<bool> hasFailed
     , ExecutionRequestOptions executionOptions = null)
        {
            var result = await _easyExecuteMain.Execute<object, object>(
                Guid.NewGuid().ToString()
              , new object()
              , (o) => { operation(); return Task.FromResult(new object()); }
              , (r) => hasFailed()
              , null
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded,
                WorkerId = result.WorkerId
            };
        }

        #endregion NO ID NO COMMAND  NO RESULT
        

                public async Task<ExecutionResult<GetWorkLogCompletedMessage>> GetWorkLogAsync(string workId = null)
        {
            try
            {
                var result = await ExecutionQueryActorRef.Ask<GetWorkLogCompletedMessage>(new GetWorkLogMessage(workId));
                return new ExecutionResult<GetWorkLogCompletedMessage>()
                {
                    Succeeded = true,
                    Result = result
                };
            }
            catch (Exception e)
            {
                return new ExecutionResult<GetWorkLogCompletedMessage>()
                {
                    Succeeded = false,
                    Errors = new List<string>() { e.Message + " - " + e.InnerException?.Message },
                    Result = new GetWorkLogCompletedMessage(new List<Worker>())
                };
            }
        }


        public async Task<ExecutionResult<GetWorkHistoryCompletedMessage>> GetWorkHistoryAsync(string workId = null)
        {
            try
            {
                var result = await ReceptionActorRef.Ask<GetWorkHistoryCompletedMessage>(new GetWorkHistoryMessage(workId));
                return new ExecutionResult<GetWorkHistoryCompletedMessage>()
                {
                    Succeeded = true,
                    Result = result
                };
            }
            catch (Exception e)
            {
                return new ExecutionResult<GetWorkHistoryCompletedMessage>()
                {
                    Succeeded = false,
                    Errors = new List<string>() { e.Message + " - " + e.InnerException?.Message },
                    Result = new GetWorkHistoryCompletedMessage(new List<Worker>(), DateTime.UtcNow)
                };
            }
        }
    }
}