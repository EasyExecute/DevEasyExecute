using EasyExecute.Common;
using EasyExecute.Messages;

namespace EasyExecute.Tests
{
    public class ExepectedTestResult
    {
        public ExecutionResult<TestClass> ExecutionResult { set; get; }
        public ExecutionResult<GetWorkHistoryCompletedMessage> ExecutionResultHistory { set; get; }
    }
}