using EasyExecute.Common;
using EasyExecute.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EasyExecute.Tests
{
   
    public class when_any_api_method_is_used
    {
        [Fact]
        public void ensure_all_api_works_has_id_no_command_has_result()
        {
            var workerId = Guid.NewGuid().ToString();
            var expectedResult = GetHappyPathExpectedResult(workerId);

            var testHappyPathRequest = GetHappyPathRequest<TestClass, string>(workerId);

            #region HAS ID NO COMMAND HAS RESULT

            RunTest(workerId, "HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , () => Task.FromResult(new TestClass(command))
                    , testHappyPathRequest.HasFailedHavingResult
                    , TimeSpan.FromSeconds(5)
                    , new ExecutionRequestOptions
                    {
                        DontCacheResultById = false
                    }).Result, expectedResult);
            RunTest(workerId, "HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , () => Task.FromResult(new TestClass(command))
                    , testHappyPathRequest.HasFailedHavingResult
                    , TimeSpan.FromSeconds(5)
                    ).Result, expectedResult);
            RunTest(workerId, "HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , () => Task.FromResult(new TestClass(command))
                    , testHappyPathRequest.HasFailedHavingResult
                    ).Result, expectedResult);
            RunTest(workerId, "HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , () => Task.FromResult(new TestClass(command))
                    ).Result, expectedResult);

            RunTest(workerId, "HAS ID - NO COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , () => Task.FromResult(new TestClass(command))
                    ).Result, expectedResult);

            #endregion HAS ID NO COMMAND HAS RESULT
        }

        [Fact]
        public void ensure_all_api_works_has_id_has_command_has_result()
        {
            var workerId = Guid.NewGuid().ToString();
            var expectedResult = GetHappyPathExpectedResult(workerId);

            var testHappyPathRequest = GetHappyPathRequest<TestClass, string>(workerId);

            #region HAS ID  HAS COMMAND HAS RESULT

            RunTest(workerId, "HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , command
                    , com => Task.FromResult(new TestClass(com))
                    , testHappyPathRequest.HasFailedHavingResult
                    , TimeSpan.FromSeconds(5)
                    , new ExecutionRequestOptions
                    {
                        DontCacheResultById = false
                    }).Result, expectedResult);
            RunTest(workerId, "HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , command
                    , com => Task.FromResult(new TestClass(com))
                    , testHappyPathRequest.HasFailedHavingResult
                    , TimeSpan.FromSeconds(5)
                    ).Result, expectedResult);
            RunTest(workerId, "HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , command
                    , com => Task.FromResult(new TestClass(com))
                    , testHappyPathRequest.HasFailedHavingResult
                    ).Result, expectedResult);

            RunTest(workerId, "HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , command
                    , com => Task.FromResult(new TestClass(com))
                    ).Result, expectedResult);

            RunTest(workerId, "HAS ID - HAS COMMAND - HAS RESULT", (service, id, command) =>
                service.ExecuteAsync(
                    id
                    , command
                    , com => Task.FromResult(new TestClass(com))
                    ).Result, expectedResult);

            #endregion HAS ID  HAS COMMAND HAS RESULT
        }

        [Fact]
        public void ensure_all_api_works_has_id_no_command_no_result()
        {
            var workerId = Guid.NewGuid().ToString();
            var expectedResult = GetHappyPathExpectedResult(workerId);

            var testHappyPathRequest = GetHappyPathRequest<TestClass, string>(workerId);

            #region HAS ID  NO COMMAND  NO RESULT

            RunTest(workerId, "HAS ID - NO COMMAND - NO RESULT", (service, id, command) =>
            {
                var result = service.ExecuteAsync(
                    id
                    , command
                    , com => Task.FromResult(new TestClass(com))
                    , testHappyPathRequest.HasFailed
                    , TimeSpan.FromSeconds(5)
                    , new ExecutionRequestOptions
                    {
                        DontCacheResultById = false
                    }).Result;

                return new ExecutionResult<TestClass>
                {
                    Succeeded = true,
                    Result = new TestClass(command),
                    Errors = new List<string>(),
                    WorkerId = id
                };
            }, expectedResult);

            #endregion HAS ID  NO COMMAND  NO RESULT
        }

        [Fact]
        public void ensure_all_api_works_no_id_no_command_no_result()
        {
            var workerId = Guid.NewGuid().ToString();
            var expectedResult = GetHappyPathExpectedResult(workerId);

            var testHappyPathRequest = GetHappyPathRequest<TestClass, string>(workerId);

            #region NO ID NO COMMAND  NO RESULT

            RunTest(workerId, "NO ID - NO COMMAND - NO RESULT", (service, id, command) =>
            {
                var result = service.ExecuteAsync(() => { }).Result;
                return new ExecutionResult<TestClass>
                {
                    Succeeded = true,
                    Result = new TestClass(command),
                    Errors = new List<string>(),
                    WorkerId = id
                };
            }, expectedResult);

            #endregion NO ID NO COMMAND  NO RESULT
        }

        private static TestHappyPathRequest<TCommand, TResult> GetHappyPathRequest<TCommand, TResult>(string workerId)
            where TResult : class
        {
            var TestHappyPathRequest = new TestHappyPathRequest<TCommand, TResult>
            {
                Id = workerId,
                Command = default(TCommand),
                Operation = null,
                MaxExecutionTimePerAskCall = null,
                ExecutionOptions = null,
                TransformResult = null,
                HasFailedHavingResult = r => false,
                HasFailed = () => false
            };
            return TestHappyPathRequest;
        }

        private static ExepectedTestResult GetHappyPathExpectedResult(string workerId)
        {
            return new ExepectedTestResult
            {
                ExecutionResult = new ExecutionResult<TestClass>
                {
                    Succeeded = true,
                    Errors = new List<string>(),
                    Result = new TestClass(workerId),
                    WorkerId = workerId
                },
                ExecutionResultHistory = new ExecutionResult<GetWorkHistoryCompletedMessage>
                {
                    Errors = new List<string>(),
                    Result = new GetWorkHistoryCompletedMessage(new List<Worker>
                    {
                        new Worker(workerId, new WorkerStatus
                        {
                            IsCompleted = true
                        }, null, null, false, DateTime.UtcNow,true,null)
                    }, DateTime.UtcNow),
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
                    Assert.Equal(expectedResult.ExecutionResultHistory.Result.WorkHistory.Count,
                        history.Result.WorkHistory.Count);
                    Assert.True(history.Result.WorkHistory.First().WorkerStatus.IsCompleted);
                }
                catch (Exception e)
                {
                    throw new Exception("Error : " + description + " - " + e.Message, e);
                }
            }
        }
    }

    

}