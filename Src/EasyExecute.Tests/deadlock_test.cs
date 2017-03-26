using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EasyExecute.Tests
{
    public class deadlock_test
    {
        [Fact]
        public void test()
        {
            var task = AsyncString();
            task.Wait();

            // This line will never be reached
            Assert.NotNull(task.Result);
        }
        public async Task<string> AsyncString()
        {
            await Task.Delay(1000);
            return "TestAsync";
        }
    }
}
