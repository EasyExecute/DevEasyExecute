using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EasyExecute.Tests
{
    public class when_any_api_method_is_used
    {
        public class TestClass
        {
            public TestClass(int data)
            {
                Data = data;
            }

            public int Data {  set; get; }
        }

        [Fact]
        public void no_command_some_result()
        {
           

            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync("1",
                () => Task.FromResult(new TestClass(1)))
                .Result;
            var history = service.GetWorkHistoryAsync().Result;

            Assert.True(result.Succeeded);
            Assert.Equal(1,result.Result.Data);
            Assert.Equal(0, result.Errors.Count);
            Assert.True( history.Result.LastSystemAccessedTime<DateTime.UtcNow);
            Assert.True(history.Succeeded);
            Assert.Equal(0,history.Errors.Count);
            Assert.Equal(1, history.Result.WorkHistory.Count);
            Assert.True(history.Result.WorkHistory.First().WorkerStatus.IsCompleted);
        }
    }
}