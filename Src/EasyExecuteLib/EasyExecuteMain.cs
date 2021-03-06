using Akka.Actor;
using EasyExecute.Common;
using EasyExecute.Messages;
using System;
using System.Threading.Tasks;

namespace EasyExecuteLib
{
    public class EasyExecuteMain
    {
        private readonly EasyExecute _easyExecute;

        public EasyExecuteMain(EasyExecute easyExecute)
        {
            _easyExecute = easyExecute;
        }

        public async Task<ExecutionResult<TResult>> Execute<TResult, TCommand>(
            string id
          , TCommand command
          , Func<TCommand, Task<TResult>> operation
          , Func<TResult, bool> hasFailed
          , TimeSpan? maxExecutionTimePerAskCall = null
          , Func<ExecutionResult<TResult>, TResult> transformResult = null
          , ExecutionRequestOptions executionOptions = null) where TResult : class
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (id == null) throw new ArgumentNullException(nameof(id));
            executionOptions = executionOptions ?? new ExecutionRequestOptions();
            executionOptions.CacheExpirationPeriod =executionOptions.CacheExpirationPeriod ?? _easyExecute.DefaultCacheExpirationPeriod;
            var maxExecTime = maxExecutionTimePerAskCall ?? _easyExecute.DefaultMaxExecutionTimePerAskCall;
            if (transformResult == null)
            {
                transformResult = (r) => r.Result;
            }
            if (hasFailed == null)
            {
                hasFailed = (r) => false;
            }

            IEasyExecuteResponseMessage result;
           
            try
            {
                var expiration = executionOptions.CacheExpirationPeriod == null
                      ? (DateTime?)null
                      : DateTime.UtcNow.AddMilliseconds(executionOptions.CacheExpirationPeriod.Value.TotalMilliseconds);

                var setWorkMessage = new SetWorkMessage(
                    id
                    , command
                    , new WorkFactory(
                          async (o) => await operation((TCommand)o).ConfigureAwait(false)
                        , (r) => (bool) hasFailed?.Invoke((TResult)r)
                        , executionOptions.MaxRetryCount)
                    , executionOptions.StoreCommands
                    , expiration
                    , executionOptions.DontCacheResultById
                    , executionOptions.OnWorkerPurged
                    , executionOptions.FailExecutionIfTaskIsCancelled);

                if (executionOptions.ExecuteReactively)
                {
                    _easyExecute.ReceptionActorRef.Tell(setWorkMessage);
                    result = new ExecuteReactivelyPlacedMessage(id);
                }
                else
                {
                    result = await _easyExecute.ReceptionActorRef.Ask<IEasyExecuteResponseMessage>(setWorkMessage, maxExecTime).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                result = new SetWorkErrorMessage(
                    $"Operation execution timed out . execution time exceeded the set max execution time of {maxExecTime.TotalMilliseconds} ms to worker id: {id} - Exception : {e.Message} - {e.InnerException?.Message ?? ""}"
                  , id
                  , null);
            }
            var finalResult = new ExecutionResult<TResult> { WorkerId = result.WorkerId };
            if (result is SetWorkErrorMessage)
            {
                var tmpResult = (result as SetWorkErrorMessage);
                finalResult.Errors.Add(tmpResult.Error);
                finalResult.Result = tmpResult.Result as TResult;
            }
            else if (result is SetCompleteWorkErrorMessage)
            {
                finalResult.Errors.Add((result as SetCompleteWorkErrorMessage).Error);
                if (!executionOptions.DontCacheResultById)
                {
                    finalResult.Result = (result as SetCompleteWorkErrorMessage)?.LastSuccessfullResult as TResult;
                }
            }
            else if (result is ExecuteReactivelyPlacedMessage)
            {
                finalResult.Succeeded = true;
            }
            else
            {
                finalResult.Succeeded = true;
                finalResult.Result = (result as SetWorkSucceededMessage)?.Result as TResult;
            }
            finalResult.Result = transformResult == null ? finalResult.Result : transformResult(finalResult);
            return finalResult;
        }
    }
}