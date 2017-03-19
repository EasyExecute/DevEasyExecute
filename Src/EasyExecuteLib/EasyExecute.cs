using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using EasyExecute.ActorSystemFactory;
using EasyExecute.Common;
using EasyExecute.Messages;
using EasyExecute.Reception;

namespace EasyExecuteLib
{
    public class EasyExecute
    {
        private readonly EasyExecuteMain _easyExecuteMain;
        private ActorSystemCreator ActorSystemCreator { get; }
        internal TimeSpan MaxExecutionTimePerAskCall { get; }
        internal IActorRef ReceptionActorRef { get; }
       
        public EasyExecute(TimeSpan? maxExecutionTimePerAskCall = null, string serverActorSystemName = null, ActorSystem actorSystem = null, string actorSystemConfig = null, TimeSpan? purgeInterval = null, Action<Worker> onWorkerPurged = null)
        {
           
            serverActorSystemName = (string.IsNullOrEmpty(serverActorSystemName) && actorSystem == null)
                ? Guid.NewGuid().ToString()
                : serverActorSystemName;
            _easyExecuteMain = new EasyExecuteMain(this);
            ActorSystemCreator = new ActorSystemCreator();
            ActorSystemCreator.CreateOrSetUpActorSystem(serverActorSystemName, actorSystem, actorSystemConfig);
            ReceptionActorRef = ActorSystemCreator.ServiceActorSystem.ActorOf(Props.Create(() => new ReceptionActor(purgeInterval, onWorkerPurged)));
            MaxExecutionTimePerAskCall = maxExecutionTimePerAskCall ?? TimeSpan.FromSeconds(5);
        }

        internal const bool DefaultReturnExistingResultWhenDuplicateId = true;


        #region NO COMMAND
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

      
        #endregion

        #region HAS COMMAND
       
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

    
        #endregion

        #region COMMAND ONLY
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
              ,null
              
              , maxExecutionTimePerAskCall
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded
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
                Succeeded = result.Succeeded
            };
        }


        #endregion

        #region FIRE AND FORGET
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
              ,null
              , maxExecutionTimePerAskCall
              , null
              , executionOptions);
            return new ExecutionResult()
            {
                Errors = result.Errors,
                Succeeded = result.Succeeded
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
                Succeeded = result.Succeeded
            };
        }
        #endregion

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