using System;
using System.Threading.Tasks;
using EasyExecute.Common;

namespace EasyExecute.Tests
{
    public class TestHappyPathRequest<TCommand, TResult> where TResult : class
    {
        public string Id { set; get; }
        public TCommand Command { set; get; }
        public Func<bool> HasFailed { set; get; }
        public Func<TCommand, bool> HasFailedHavingResult { set; get; }
        public Func<TCommand, Task<TResult>> Operation { set; get; }
        public TimeSpan? MaxExecutionTimePerAskCall { set; get; }
        public ExecutionRequestOptions ExecutionOptions { set; get; }
        public Func<ExecutionResult<TResult>, TResult> TransformResult { set; get; }
    }

    public class TestHappyPathRequest<TCommand> : TestHappyPathRequest<TCommand, object>
    {
    }

    public class TestHappyPathRequest : TestHappyPathRequest<object, object>
    {
    }
}