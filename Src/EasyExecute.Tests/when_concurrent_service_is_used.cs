using EasyExecute.Messages;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EasyExecute.Common;
using Xunit;
using Xunit.Sdk;

namespace EasyExecute.Tests
{
    public class when_concurrent_service_is_used
    {

        [Fact]
        public void test()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var t= ( service.ExecuteAsync("1", () =>
                {
                    return  Task.FromResult(new object());
                }, (finalResult) =>
                {
                    return false;
                }, 
                TimeSpan.FromSeconds(5), new ExecutionRequestOptions()
                {
                    ReturnExistingResultWhenDuplicateId = true
                }, (executionResult) =>
                {
                    return executionResult.Result;
                })).Result.Result;

            Assert.NotNull(t);
            var history=  service.GetWorkHistoryAsync().Result;
            Assert.True(history.Result.WorkHistory.First().WorkerStatus.IsCompleted);
        }
        [Fact]
        public void test3()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var t = (service.ExecuteAsync("1",DateTime.UtcNow, (d) =>
            {
                return Task.FromResult(new {time=d});
            }, (finalResult) =>
            {
                return false;
            }, 
                TimeSpan.FromSeconds(5), new ExecutionRequestOptions()
                {
                    ReturnExistingResultWhenDuplicateId = true
                }, (executionResult) =>
                {
                    return executionResult.Result;
                })).Result.Result;

            Assert.NotNull(t);
            var history = service.GetWorkHistoryAsync().Result;
            Assert.True(history.Result.WorkHistory.First().WorkerStatus.IsCompleted);
        }
        [Fact]
        public void test2()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var t = (service.ExecuteAsync("1", () =>
            {
                return Task.FromResult(new object());
            }, (finalResult) =>
            {
                return false;
            },  TimeSpan.FromSeconds(5), new ExecutionRequestOptions()
            {
                ReturnExistingResultWhenDuplicateId = true
            })).Result.Result;

            Assert.NotNull(t);
        }

        [Fact]
        public void test_max_execution_time_not_exceeded()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", () =>
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait(); return Task.FromResult(new object());
            }, (r) => false,  TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                ReturnExistingResultWhenDuplicateId = true
            }).Result;
            Assert.True(result.Succeeded);
        }

        [Fact]
        public void test_max_execution_time_not_exceeded2()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", async () =>
            {
                await  Task.Delay(TimeSpan.FromSeconds(1)); return new object();
            }, (r) => false,  TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                ReturnExistingResultWhenDuplicateId = true
            }).Result;
            Assert.True(result.Succeeded);
        }

        [Fact]
        public void test_max_execution_time_not_exceeded3()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1)); return Task.FromResult(new object());
            }, (r) => false,  TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                ReturnExistingResultWhenDuplicateId = true
            }).Result;
            Assert.True(result.Succeeded);
        }


        [Fact]
        public void test_max_execution_time_exceeded_should_return_correct_value_when_called_again()
        {
            var now = DateTime.UtcNow;
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                return await Task.FromResult(now);
            }, (r) => false,  TimeSpan.FromSeconds(1), new ExecutionRequestOptions()
            {
                ReturnExistingResultWhenDuplicateId = true
            }).Result;
            Assert.False(result.Succeeded);
            Assert.NotEqual(now, result.Result);
            SimulateNormalClient();
            SimulateNormalClient();
           
            result = service.ExecuteAsync<object>("1", async () =>
            {
                await  Task.Delay(TimeSpan.FromSeconds(1)); return await Task.FromResult(DateTime.UtcNow);
            }, (r) => false,  TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                ReturnExistingResultWhenDuplicateId = true
            }).Result;

            Assert.False(result.Succeeded);
            Assert.Equal(now,result.Result);

        }


        [Fact]
        public void test_max_execution_time_exceeded()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", () =>
            {
                Task.Delay(TimeSpan.FromSeconds(6)).Wait(); return Task.FromResult(new object());
            }, (r) => false,  TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                ReturnExistingResultWhenDuplicateId = true
            }).Result;
            Assert.False(result.Succeeded);
        }

        [Fact]
        public void test_max_execution_time_exceeded2()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", async() =>
            {
                await  Task.Delay(TimeSpan.FromSeconds(6)); return new object();
            }, (r) => false, TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                ReturnExistingResultWhenDuplicateId = true
            }).Result;
            Assert.False(result.Succeeded);
        }

        [Fact]
        public void test_max_execution_time_exceeded3()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(6)); return Task.FromResult(new object());
            }, (r) => false, TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                ReturnExistingResultWhenDuplicateId = true
            }).Result;
            Assert.False(result.Succeeded);
        }

        [Fact]
        public void it_should_block_duplicate_work_id()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result =
                          service.ExecuteAsync<object>("1", () => Task.FromResult(new object())).Result;
            Assert.True(result.Succeeded);
            SimulateNormalClient();

            var result2 =
                         service.ExecuteAsync<object>("1", () => Task.FromResult(new object())).Result;
            Assert.False(result2.Succeeded);
        }

        private static void SimulateNormalClient()
        {
//simulating fast client send
            System.Threading.Thread.Sleep(1000);
            System.Threading.Thread.Sleep(1000);
            System.Threading.Thread.Sleep(1000);
        }

        [Fact]
        public void it_should_fail_when_client_markes_result_as_failure()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", () => Task.FromResult(new object()), (r) => true).Result;
            Assert.False(result.Succeeded);
            SimulateNormalClient();

            var result2 = service.ExecuteAsync<object>("1", () => Task.FromResult(new object())).Result;
            Assert.True(result2.Succeeded);
        }

        [Fact]
        public void it_should_fail_when_client_markes_result_as_failure2()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result =
                          service.ExecuteAsync<object>("1", () => Task.FromResult(new object())).Result;
            Assert.True(result.Succeeded);
            var result2 =
                         service.ExecuteAsync<object>("1", () => Task.FromResult(new object()), (r) => true).Result;
            Assert.False(result2.Succeeded);
        }

        [Fact]
        public void it_should_fail_when_client_markes_result_as_failure_even_with_different_id()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result =
                          service.ExecuteAsync<object>("1", () => Task.FromResult(new object())).Result;
            Assert.True(result.Succeeded);
            var result2 =
                         service.ExecuteAsync<object>("2", () => Task.FromResult(new object()), (r) => true).Result;
            Assert.False(result2.Succeeded);
        }

        [Fact]
        public void it_should_fail_when_client_markes_result_as_failure3()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result =
                          service.ExecuteAsync<object>("1", () => Task.FromResult(new object()), (r) => false).Result;
            Assert.True(result.Succeeded);
            var result2 =
                         service.ExecuteAsync<object>("1", () => Task.FromResult(new object()), (r) => true).Result;
            Assert.False(result2.Succeeded);
        }

        [Fact]
        public void it_should_fail_when_client_markes_result_as_failure_even_with_different_id2()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result =
                          service.ExecuteAsync<object>("1", () => Task.FromResult(new object()), (r) => false).Result;
            Assert.True(result.Succeeded);
            var result2 =
                         service.ExecuteAsync<object>("2", () => Task.FromResult(new object()), (r) => true).Result;
            Assert.False(result2.Succeeded);
        }

        [Fact]
        public void it_should_pass_when_client_markes_result_as_failure_with_different_id()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result =
                          service.ExecuteAsync<object>("1", () => Task.FromResult(new object())).Result;
            Assert.True(result.Succeeded);
            var result2 =
                         service.ExecuteAsync<object>("2", () => Task.FromResult(new object()), (r) => false).Result;
            Assert.True(result2.Succeeded);
        }

        [Fact]
        public void it_should_pass_when_client_markes_result_as_failure_with_different_id2()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result =
                          service.ExecuteAsync<object>("1"
                          , () => Task.FromResult(new object()), (r) => false).Result;
            Assert.True(result.Succeeded);
            var result2 =
                         service.ExecuteAsync<object>("2", () => Task.FromResult(new object())).Result;
            Assert.True(result2.Succeeded);
        }

        [Fact]
        public void it_should_run_many_operations_fast()
        {
            const int numberOfRequests = 2000;
            var maxExecutionTimePerAskCall = TimeSpan.FromSeconds(3);
            const int maxTotalExecutionTimeMs = 4000;
            var service = new EasyExecuteLib.EasyExecute(maxExecutionTimePerAskCall);
            var watch = Stopwatch.StartNew();
            Parallel.ForEach(Enumerable.Range(0, numberOfRequests),
                basket =>
                {
                    var result =
                        service.ExecuteAsync<object>(basket.ToString(), () => Task.FromResult(new object())).Result;
                });
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Assert.True(elapsedMs < maxTotalExecutionTimeMs,
                $"Test took {elapsedMs} ms which is more than {maxTotalExecutionTimeMs}");
        }

        [Fact]
        public void it_should_run_one_operations_sequentially()
        {
            const int numberOfBaskets = 100;
            const int numberOfPurchaseFromOneBasketCount = 1;
            var maxExecutionTimePerAskCall = TimeSpan.FromSeconds(5);
            var service = new EasyExecuteLib.EasyExecute(maxExecutionTimePerAskCall);

            var durationMs = TestHelper.TestOperationExecution(numberOfBaskets, numberOfPurchaseFromOneBasketCount,
                (basketIds, purchaseService) =>
                {
                    Parallel.ForEach(basketIds,
                        basketId =>
                        {
                            var result =
                                service.ExecuteAsync(basketId, () =>  purchaseService.RunPurchaseServiceAsync(basketId)).Result;
                        });
                },
                maxExecutionTimePerAskCall);
            Console.WriteLine($"Test took {durationMs} ms");
        }

        [Fact]
        public void it_should_run_many_operations_sequentially()
        {
            const int numberOfBaskets = 100;
            const int numberOfPurchaseFromOneBasketCount = 10;
            var maxExecutionTimePerAskCall = TimeSpan.FromSeconds(5);
            var service = new EasyExecuteLib.EasyExecute(maxExecutionTimePerAskCall);

            TestHelper.TestOperationExecution(numberOfBaskets, numberOfPurchaseFromOneBasketCount,
                (basketIds, purchaseService) =>
                {
                    Parallel.ForEach(basketIds,
                        basketId =>
                        {
                            var result =
                                service.ExecuteAsync(basketId, () => purchaseService.RunPurchaseServiceAsync(basketId)).Result;
                        });
                },
                maxExecutionTimePerAskCall);
        }

        [Fact]
        public void it_should_fail_torun_many_operations_sequentially_without_helper()
        {
            const int numberOfBaskets = 100;
            const int numberOfPurchaseFromOneBasketCount = 10;
            var maxExecutionTimePerAskCall = TimeSpan.FromSeconds(5);
            var service = new EasyExecuteLib.EasyExecute(maxExecutionTimePerAskCall);

            Assert.Throws<AllException>(() =>
            {
                TestHelper.TestOperationExecution(numberOfBaskets, numberOfPurchaseFromOneBasketCount,
                    (basketIds, purchaseService) =>
                    {
                        Parallel.ForEach(basketIds, basketId => { purchaseService.RunPurchaseServiceAsync(basketId); });
                    },
                    maxExecutionTimePerAskCall);
            });
        }
    }
}