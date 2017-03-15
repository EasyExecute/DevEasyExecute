using EasyExecute.Messages;
using System;
using System.Threading.Tasks;

namespace EasyExecute.Common
{
    public class WorkFactory : IWorkFactory
    {
        public WorkFactory(Func<object,Task<object>> operation, Func<object, bool> hasFailed)
        {
            OperationAsync = operation;
            RunAsyncMethod = true;
            HasFailed = hasFailed;
        }

        private Func<object> Operation { get; }
        private Func<object, Task<object>> OperationAsync { get; }

        //public WorkFactory(Func<object> operation )
        //{
        //    Operation = operation;
        //}
        public object Execute()
        {
            return Operation();
        }

        public Task<object> ExecuteAsync(object command)
        {
            return OperationAsync(command);
        }

       
        public bool IsAFailedResult(object result)
        {
            return (HasFailed != null && HasFailed(result));
        }

        public bool RunAsyncMethod { get; set; }
        private Func<object, bool> HasFailed { get; }
    }
}