using System;
using System.Threading.Tasks;
using Akka.Actor;
using EasyExecute.Common;
using EasyExecute.Messages;

namespace EasyExecuteLib
{
    public class EasyExecuteMain
    {
        private EasyExecute _easyExecute;

        public EasyExecuteMain(EasyExecute easyExecute)
        {
            _easyExecute = easyExecute;
        }

        public async Task<ExecutionResult<TResult>> Execute<TResult, TCommand>(string id, TCommand command, Func<TCommand,Task<TResult>> operation, Func<TResult,bool> hasFailed=null, bool returnExistingResultWhenDuplicateId=true, TimeSpan? maxExecutionTimePerAskCall = null,Func<ExecutionResult<TResult>, TResult> transformResult=null,bool storeCommands=false ) where TResult : class
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (id == null) throw new ArgumentNullException(nameof(id));

            IEasyExecuteResponseMessage result;
            var maxExecTime = maxExecutionTimePerAskCall ?? _easyExecute.MaxExecutionTimePerAskCall;
            try
            {
                result = await _easyExecute.ReceptionActorRef.Ask<IEasyExecuteResponseMessage>(new SetWorkMessage(id, command, new WorkFactory(async (o)=> await operation((TCommand)o), (r) => hasFailed?.Invoke((TResult)r) ?? false),storeCommands), maxExecTime).ConfigureAwait(false);

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
    }
}