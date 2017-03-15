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
        public EasyExecute(TimeSpan? maxExecutionTimePerAskCall = null, string serverActorSystemName = null, ActorSystem actorSystem = null, string actorSystemConfig = null, TimeSpan? purgeInterval = null, Action<Worker> onWorkerPurged = null)
        {
            ActorSystemCreator = new ActorSystemCreator();

            if (string.IsNullOrEmpty(serverActorSystemName) && actorSystem == null)
            {
                serverActorSystemName = Guid.NewGuid().ToString();
            }

            ActorSystemCreator.CreateOrSetUpActorSystem(serverActorSystemName, actorSystem, actorSystemConfig);
            ReceptionActorRef = ActorSystemCreator.ServiceActorSystem.ActorOf(Props.Create(() => new ReceptionActor(purgeInterval, onWorkerPurged)));
            MaxExecutionTimePerAskCall = maxExecutionTimePerAskCall ?? TimeSpan.FromSeconds(5);
        }

        private ActorSystemCreator ActorSystemCreator { get; }
        private TimeSpan MaxExecutionTimePerAskCall { get; }

        private IActorRef ReceptionActorRef { get; }

        //public async Task ExecuteAsync(Action operation, string id, Func<object, bool> hasFailed = null, bool returnExistingResultWhenDuplicateId = true, TimeSpan? maxExecutionTimePerAskCall = null, Func<ExecutionResult<object>, ExecutionResult<object>> transformResult = null)
        //{
        //    await ExecuteAsync<object>(async () =>
        //    {
        //        operation();
        //        return await Task.FromResult(new object());
        //    }, id, hasFailed, returnExistingResultWhenDuplicateId, maxExecutionTimePerAskCall, transformResult);
        //}



        public Task<ExecutionResult<TResult>> ExecuteAsync<TResult>(string id,  Func<Task<TResult>> operation, Func<TResult, bool> hasFailed = null, bool returnExistingResultWhenDuplicateId = true, TimeSpan? maxExecutionTimePerAskCall = null, Func<ExecutionResult<TResult>, TResult> transformResult = null, bool storeCommands = false) where TResult : class
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (id == null) throw new ArgumentNullException(nameof(id));
            return Execute(id, new object(), (o)=> operation(), hasFailed, returnExistingResultWhenDuplicateId, maxExecutionTimePerAskCall, transformResult,storeCommands);
        }
        public Task<ExecutionResult<TResult>> ExecuteAsync<TResult,TCommand>(string id, TCommand command, Func<TCommand,Task<TResult>> operation, Func<TResult, bool> hasFailed=null, bool returnExistingResultWhenDuplicateId=true, TimeSpan? maxExecutionTimePerAskCall = null, Func<ExecutionResult<TResult>,TResult> transformResult = null,bool storeCommands= false) where TResult : class
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (id == null) throw new ArgumentNullException(nameof(id));
            return Execute(id,command, operation, hasFailed, returnExistingResultWhenDuplicateId, maxExecutionTimePerAskCall, transformResult,storeCommands);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="id"></param>
        /// <param name="operation"></param>
        /// <param name="hasFailed"></param>
        /// <param name="returnExistingResultWhenDuplicateId"></param>
        /// <param name="maxExecutionTimePerAskCall"></param>
        /// <param name="transformResult">Also passed boolean if error contains existing result</param>
        /// <returns></returns>
        private async Task<ExecutionResult<TResult>> Execute<TResult, TCommand>(string id, TCommand command, Func<TCommand,Task<TResult>> operation, Func<TResult,bool> hasFailed=null, bool returnExistingResultWhenDuplicateId=true, TimeSpan? maxExecutionTimePerAskCall = null,Func<ExecutionResult<TResult>, TResult> transformResult=null,bool storeCommands=false ) where TResult : class
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (id == null) throw new ArgumentNullException(nameof(id));

            IEasyExecuteResponseMessage result;
            var maxExecTime = maxExecutionTimePerAskCall ?? MaxExecutionTimePerAskCall;
            try
            {
                result = await ReceptionActorRef.Ask<IEasyExecuteResponseMessage>(new SetWorkMessage(id, command, new WorkFactory(async (o)=> await operation((TCommand)o), (r) => hasFailed?.Invoke((TResult)r) ?? false),storeCommands), maxExecTime).ConfigureAwait(false);

            }
            catch (Exception e)
            {
                result= new SetWorkErrorMessage($"Operation execution timed out . execution time exceeded the set max execution time of {maxExecTime.TotalMilliseconds} ms to worker id: {id} ",id);
            }
            var finalResult = new ExecutionResult<TResult>();
           
            if (result is SetWorkErrorMessage)
            {
                finalResult.Errors.Add((result as SetWorkErrorMessage).Error);
            }
            else if(result is SetCompleteWorkErrorMessage)
            {
                finalResult.Errors.Add((result as SetCompleteWorkErrorMessage).Error);
                if (returnExistingResultWhenDuplicateId)
                {
                    finalResult.Result = (result as SetCompleteWorkErrorMessage)?.LastSuccessfullResult as TResult;
                
                }
            }
            else
            {
                finalResult.Succeeded = true;
                finalResult.Result = (result as SetWorkSucceededMessage)?.Result as TResult;
            }
            finalResult.Result= transformResult == null ? finalResult.Result : transformResult(finalResult);
            return finalResult;
        }

        public async Task<ExecutionResult<GetWorkHistoryCompletedMessage>> GetWorkHistoryAsync(string workId=null)
        {
            try
            {
                var result = await ReceptionActorRef.Ask<GetWorkHistoryCompletedMessage>(new GetWorkHistoryMessage(workId??null))
                    .ConfigureAwait(false);
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
                    Errors = new List<string>() { e.Message+" - "+e.InnerException?.Message},
                    Result = new GetWorkHistoryCompletedMessage(new List<Worker>(),DateTime.UtcNow)
                };

            }
        }


    }
}