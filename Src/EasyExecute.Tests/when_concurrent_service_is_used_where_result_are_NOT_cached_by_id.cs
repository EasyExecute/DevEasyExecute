using Akka.Actor;
using EasyExecute.Common;
using EasyExecuteLib;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EasyExecute.Tests
{
    public class when_result_are_NOT_cached_by_id
    {
        [Fact]
        public void test_reactive()
        {
            var total = 100;
            var counter = 0;
            var service = new EasyExecuteLib.EasyExecute(new EasyExecuteOptions()
            {
                actorSystem = ActorSystem.Create("ConcurrentExecutorServiceActorSystem", "akka { } akka.actor{ }  akka.remote {  } "),
                purgeInterval = TimeSpan.FromSeconds(5)
            });

            var result = service.ExecuteAsync("1", async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                counter++;
                return new object();
            }, (r) => counter < total + 2, TimeSpan.FromHours(1), new ExecutionRequestOptions()
            {
                CacheExpirationPeriod = TimeSpan.FromSeconds(10),
                DontCacheResultById = true,
                StoreCommands = true,
                MaxRetryCount = total,
                ExecuteReactively = true
            }, (r) => r.Result).Result;
            Assert.True(result.Succeeded);

            bool hasRunAll;
            var retryCount = 0;
            do
            {
                hasRunAll = service.GetWorkHistoryAsync("1").Result.Result.WorkHistory.Any(x => x.WorkerStatus.Succeeded);
                Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();

                retryCount++;
                if (retryCount > 1000) break;
            } while (!hasRunAll);
            Assert.True(total + 1 == counter);
            Assert.True(retryCount > 0);
        }

        [Fact]
        public void it_should_be_able_to_retry3()
        {
            var total = 1000;
            var counter = 0;
            var service = new EasyExecuteLib.EasyExecute(new EasyExecuteOptions()
            {
                actorSystem = ActorSystem.Create("ConcurrentExecutorServiceActorSystem", "akka { } akka.actor{ }  akka.remote {  } "),
                purgeInterval = TimeSpan.FromSeconds(5)
            });
            foreach (var i in Enumerable.Range(1, total))
            {
                var result = service.ExecuteAsync("1", async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                    counter++;
                    return new object();
                }, (r) => i < total + 1, TimeSpan.FromHours(1), new ExecutionRequestOptions()
                {
                    CacheExpirationPeriod = TimeSpan.FromSeconds(10),
                    DontCacheResultById = true,
                    StoreCommands = true
                }, (r) => r.Result).Result;
                Assert.False(result.Succeeded);
                Assert.True(counter == i);
            }
        }

        [Fact]
        public void it_should_be_able_to_retry()
        {
            var counter = 0;
            var service = new EasyExecuteLib.EasyExecute(new EasyExecuteOptions()
            {
                actorSystem = ActorSystem.Create("ConcurrentExecutorServiceActorSystem", "akka { } akka.actor{ }  akka.remote {  } "),
                purgeInterval = TimeSpan.FromSeconds(5)
            });
            var result = service.ExecuteAsync("1", async () =>
            {
                counter++;
                return await Task.FromResult(new object());
            }, (r) => counter < 4, TimeSpan.FromHours(1), new ExecutionRequestOptions()
            {
                CacheExpirationPeriod = TimeSpan.FromSeconds(10),
                DontCacheResultById = true,
                StoreCommands = true,
                MaxRetryCount = 4
            }, (r) => r.Result).Result;
            Assert.True(result.Succeeded);
            Assert.True(counter == 4);

            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            result = service.ExecuteAsync("1", async () =>
            {
                counter++;
                return await Task.FromResult(new object());
            }, (r) => counter < 4, TimeSpan.FromHours(1), new ExecutionRequestOptions()
            {
                CacheExpirationPeriod = TimeSpan.FromSeconds(10),
                DontCacheResultById = true,
                StoreCommands = true,
                MaxRetryCount = 4
            }, (r) => r.Result).Result;
            Assert.True(result.Succeeded);
            Assert.True(counter == 5);
            Task.Delay(TimeSpan.FromSeconds(10)).Wait();

            result = service.ExecuteAsync("1", async () =>
            {
                counter++;
                return await Task.FromResult(new object());
            }, (r) => counter < 4, TimeSpan.FromHours(1), new ExecutionRequestOptions()
            {
                CacheExpirationPeriod = TimeSpan.FromSeconds(10),
                DontCacheResultById = true,
                StoreCommands = true,
                MaxRetryCount = 4
            }, (r) => r.Result).Result;
            Assert.True(result.Succeeded);
            Assert.True(counter == 6);
        }

        [Fact]
        public void it_should_be_able_to_set_cache_expiration_per_call()
        {
            var service = new EasyExecuteLib.EasyExecute(new EasyExecuteOptions()
            {
                actorSystem = ActorSystem.Create("ConcurrentExecutorServiceActorSystem", "akka { } akka.actor{ }  akka.remote {  } "),
                purgeInterval = TimeSpan.FromSeconds(5)
            });
            var result = service.ExecuteAsync("1", async () =>
            {
                return await Task.FromResult(new object());
            }, (r) => false, TimeSpan.FromHours(1), new ExecutionRequestOptions()
            {
                CacheExpirationPeriod = TimeSpan.FromSeconds(10),
                DontCacheResultById = true,
                StoreCommands = true
            }, (r) => r.Result).Result;

            Assert.True(result.Succeeded);
            result = service.ExecuteAsync("1", async () =>
            {
                return await Task.FromResult(new object());
            }, (r) => false, TimeSpan.FromHours(1), new ExecutionRequestOptions()
            {
                CacheExpirationPeriod = TimeSpan.FromSeconds(10),
                DontCacheResultById = true,
                StoreCommands = true
            }, (r) => r.Result).Result;

            Assert.True(result.Succeeded);

            result = service.ExecuteAsync("1", async () =>
            {
                return await Task.FromResult(new object());
            }, (r) => false, TimeSpan.FromHours(1), new ExecutionRequestOptions()
            {
                CacheExpirationPeriod = TimeSpan.FromSeconds(10),
                DontCacheResultById = true,
                StoreCommands = true
            }, (r) => r.Result).Result;

            Assert.True(result.Succeeded);
        }

        [Fact]
        public void return_execution_history()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var t = service.ExecuteAsync("1",
                () => Task.FromResult(new object()),
                (finalResult) => false,
                TimeSpan.FromSeconds(5), new ExecutionRequestOptions()
                {
                    DontCacheResultById = true
                }, (executionResult) => executionResult.Result).Result.Result;

            Assert.NotNull(t);
            var history = service.GetWorkHistoryAsync().Result;
            Assert.True(history.Result.WorkHistory.Count == 0);
            var logs = service.GetWorkLogAsync().Result;
            Assert.True(logs.Result.WorkLog.Count == 7);
        }

        [Fact]
        public void test3()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var t = (service.ExecuteAsync("1", DateTime.UtcNow,
                (d) => Task.FromResult(new { time = d }),
                (finalResult) => false,
                TimeSpan.FromSeconds(5), new ExecutionRequestOptions()
                {
                    DontCacheResultById = true
                }, (executionResult) => executionResult.Result)).Result.Result;

            Assert.NotNull(t);
            var history = service.GetWorkHistoryAsync().Result;
            Assert.True(history.Result.WorkHistory.Count == 0);
        }

        [Fact]
        public void test_max_execution_time_not_exceeded()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", () =>
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait(); return Task.FromResult(new object());
            }, (r) => false, TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                DontCacheResultById = true
            }).Result;
            Assert.True(result.Succeeded);
        }

        [Fact]
        public void test_max_execution_time_not_exceeded2()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1)); return new object();
            }, (r) => false, TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                DontCacheResultById = true
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
            }, (r) => false, TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                DontCacheResultById = true
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
            }, (r) => false, TimeSpan.FromSeconds(1), new ExecutionRequestOptions()
            {
                DontCacheResultById = true,
            }).Result;
            Assert.False(result.Succeeded);
            Assert.NotEqual(now, result.Result);
            SimulateNormalClient();
            SimulateNormalClient();

            result = service.ExecuteAsync<object>("1", async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1)); return await Task.FromResult(DateTime.UtcNow);
            }, (r) => false, TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                DontCacheResultById = true,
            }).Result;

            Assert.True(result.Succeeded);
            Assert.NotEqual(now, result.Result);
        }

        [Fact]
        public void test_max_execution_time_exceeded()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", () =>
            {
                Task.Delay(TimeSpan.FromSeconds(6)).Wait(); return Task.FromResult(new object());
            }, (r) => false, TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                DontCacheResultById = true,
            }).Result;
            Assert.False(result.Succeeded);
        }

        [Fact]
        public void test_max_execution_time_exceeded2()
        {
            var service = new EasyExecuteLib.EasyExecute();
            var result = service.ExecuteAsync<object>("1", async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(6)); return new object();
            }, (r) => false, TimeSpan.FromSeconds(3), new ExecutionRequestOptions()
            {
                DontCacheResultById = true,
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
                DontCacheResultById = true,
            }).Result;
            Assert.False(result.Succeeded);
        }

        private static void SimulateNormalClient()
        {
            //simulating fast client send
            System.Threading.Thread.Sleep(1000);
            System.Threading.Thread.Sleep(1000);
            System.Threading.Thread.Sleep(1000);
        }
    }
}