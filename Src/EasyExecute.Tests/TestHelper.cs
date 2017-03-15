using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Util.Internal;
using EasyExecute.Messages;
using EasyExecute.Tests.TestSystem;
using Xunit;

namespace EasyExecute.Tests
{
    public class TestHelper
    {
        public static long TestOperationExecution(int numberOfBaskets, int numberOfPurchaseFromOneBasketCount,Action<List<string>,IPurchaseServiceFactory> executor,
            TimeSpan maxExecutionTimePerAskCall)
        {
            //Arrange
            var baskets = new ConcurrentDictionary<string, bool>();
            var orders = new ConcurrentDictionary<string, string>();

            //Act - obtain expected result
            for (var i = 0; i < numberOfBaskets; i++)
                baskets[Guid.NewGuid().ToString()] = true;

            var basketIds = ObtainBasketIds(baskets, numberOfPurchaseFromOneBasketCount);

            var purchaseService = new DelayedPurchaseServiceFactory(baskets, orders);
            basketIds.ForEach(basket => { var result = purchaseService.RunPurchaseServiceAsync(basket).Result; });

            //var expected = baskets.Select(b => b.Key);
            Assert.All(baskets, b => Assert.Equal(1, orders.Count(o => o.Value == b.Key)));

            //undo
            baskets.ForEach(b => { baskets[b.Key] = true; });
            orders = new ConcurrentDictionary<string, string>();

           
            purchaseService = new DelayedPurchaseServiceFactory(baskets, orders);
            var watch = Stopwatch.StartNew();

            executor(basketIds, purchaseService);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Assert.All(baskets, b => Assert.Equal(1, orders.Count(o => o.Value == b.Key)));


            return elapsedMs;
        }


        private static List<string> ObtainBasketIds(ConcurrentDictionary<string, bool> baskets,
            int numberOfPurchaseFromOneBasketCount)
        {
            var basketIds = new List<string>();
            foreach (var keyValuePair in baskets)
                for (var i = 0; i < numberOfPurchaseFromOneBasketCount; i++)
                    basketIds.Add(keyValuePair.Key);
            return basketIds;
        }
    }
}