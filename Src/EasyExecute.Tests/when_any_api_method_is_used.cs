using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyExecute.Common;
using EasyExecute.Messages;
using Xunit;

namespace EasyExecute.Tests
{
    public class when_any_api_method_is_used
    {
        public class TestClass
        {
            public TestClass(string data)
            {
                Data = data;
            }

            public string Data { set; get; }
        }

        [Fact]
        public void ensure_all_api_works()
        {
            //default options
           var workerId = Guid.NewGuid().ToString();
            var expectedResult = GetHappyPathExpectedResult(workerId);
            Func<bool> hasFailed = () => false;
            Func<TestClass,bool> hasFailedHavingResult = (r) => false;

            RunTest(workerId,"HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
             service.ExecuteAsync(
                 id
               , command
               , (com) => Task.FromResult(new TestClass(com))
               , hasFailedHavingResult
               , TimeSpan.FromSeconds(5)
               , new ExecutionRequestOptions()
               {
                   ReturnExistingResultWhenDuplicateId = true
               }).Result,expectedResult);

            RunTest(workerId,"HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
            service.ExecuteAsync(
                id
              , () => Task.FromResult(new TestClass(command))
              , hasFailedHavingResult
              , TimeSpan.FromSeconds(5)
              , new ExecutionRequestOptions()
              {
                  ReturnExistingResultWhenDuplicateId = true
              }).Result,expectedResult);

            RunTest(workerId, "HAS ID - NO COMMAND - NO RESULT", (service, id, command) =>
            {
              var result=  service.ExecuteAsync(
                    id
                    , command
                    , (com) => Task.FromResult(new TestClass(com))
                    , hasFailed
                    , TimeSpan.FromSeconds(5)
                    , new ExecutionRequestOptions()
                    {
                        ReturnExistingResultWhenDuplicateId = true
                    }).Result;

                return new ExecutionResult<TestClass>()
                {
                    Succeeded = true,
                    Result = new TestClass(command),
                    Errors = new List<string>(),
                    WorkerId = id
                };

            }, expectedResult);

            RunTest(workerId,"HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
             service.ExecuteAsync(
                 id
               , command
               , (com) => Task.FromResult(new TestClass(com))
               , hasFailedHavingResult
               , TimeSpan.FromSeconds(5)
               ).Result,expectedResult);

            RunTest(workerId,"HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
            service.ExecuteAsync(
                id
              , () => Task.FromResult(new TestClass(command))
              , hasFailedHavingResult
              , TimeSpan.FromSeconds(5)
              ).Result,expectedResult);


            RunTest(workerId,"HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
             service.ExecuteAsync(
                 id
               , command
               , (com) => Task.FromResult(new TestClass(com))
               , hasFailedHavingResult
               ).Result,expectedResult);

            RunTest(workerId,"HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
             service.ExecuteAsync(
                 id
               , () => Task.FromResult(new TestClass(command))
               , hasFailedHavingResult
               ).Result,expectedResult);

            RunTest(workerId,"HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
             service.ExecuteAsync(
                 id
               , command
               , (com) => Task.FromResult(new TestClass(com))
               ).Result,expectedResult);

            RunTest(workerId,"HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
            service.ExecuteAsync(
                id
              , () => Task.FromResult(new TestClass(command))
              ).Result,expectedResult);

            RunTest(workerId,"HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
            service.ExecuteAsync(
                id
              , command
              , (com) => Task.FromResult(new TestClass(com))
              ).Result,expectedResult);

            RunTest(workerId,"HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
             service.ExecuteAsync(
                 id
               , () => Task.FromResult(new TestClass(command))
               ).Result,expectedResult);

            RunTest(workerId,"NO ID - NO COMMAND - NO RESULT", (service, id, command) =>
            {
                var result = service.ExecuteAsync(() => { }).Result;
                return new ExecutionResult<TestClass>()
                {
                    Succeeded = true,
                    Result = new TestClass(command),
                    Errors = new List<string>(),
                    WorkerId = id
                };
            },expectedResult);
        }

        private static ExepectedTestResult GetHappyPathExpectedResult(string workerId)
        {
            return new ExepectedTestResult()
            {
                ExecutionResult = new ExecutionResult<TestClass>()
                {
                    Succeeded = true,
                    Errors = new List<string>(),
                    Result = new TestClass(workerId),
                    WorkerId = workerId
                },
                ExecutionResultHistory = new ExecutionResult<GetWorkHistoryCompletedMessage>()
                {
                    Errors = new List<string>(),
                    Result = new GetWorkHistoryCompletedMessage(new List<Worker>()
                    {
                        new Worker(workerId, new WorkerStatus()
                        {
                            IsCompleted = true
                        } , null,null,false)
                    },DateTime.UtcNow ),
                    WorkerId = null
                }
            };
        }

        private static void RunTest(string i,
            string description
          , Func<EasyExecuteLib.EasyExecute, string, string, ExecutionResult<TestClass>> operation
          , ExepectedTestResult expectedResult)
        {
           // foreach (var i in Enumerable.Range(1, 2))
            {
                
                try
                {
                    var service = new EasyExecuteLib.EasyExecute();
                    var result = operation(service, i, i);
                    var history = service.GetWorkHistoryAsync().Result;

                    Assert.True(result.Succeeded);
                    Assert.Equal(expectedResult.ExecutionResult.Result.Data, result.Result.Data);
                    Assert.Equal(expectedResult.ExecutionResult.WorkerId, result.WorkerId);
                    Assert.Equal(expectedResult.ExecutionResult.Errors.Count, result.Errors.Count);
                    Assert.True(history.Succeeded);
                    Assert.Equal(expectedResult.ExecutionResultHistory.Errors.Count, history.Errors.Count);
                    Assert.Equal(expectedResult.ExecutionResultHistory.Result.WorkHistory.Count, history.Result.WorkHistory.Count);
                    Assert.True(history.Result.WorkHistory.First().WorkerStatus.IsCompleted);
                }
                catch (Exception e)
                {
                    throw new Exception("Error : " + description + " - " + e.Message, e);
                }
            }

        }
        public class ExepectedTestResult
        {
            public ExecutionResult<TestClass> ExecutionResult { set; get; }
            public ExecutionResult<GetWorkHistoryCompletedMessage> ExecutionResultHistory { set; get; }
        }
    }
}