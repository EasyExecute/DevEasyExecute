using System;
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
        public void test()
        {
            
        }
    }
}