using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EasyExecute.Tests.TestSystem
{
    public class DelayedPurchaseServiceFactory : IPurchaseServiceFactory
    {
        public DelayedPurchaseServiceFactory(ConcurrentDictionary<string, bool> baskets,
            ConcurrentDictionary<string, string> orders)
        {
            Orders = orders;
            Baskets = baskets;
        }

        private ConcurrentDictionary<string, bool> Baskets { get; }
        private ConcurrentDictionary<string, string> Orders { get; }

        public async Task<object> RunPurchaseServiceAsync(string o)
        {
            var canBuy = Baskets[o];
            if (canBuy)
            {
                //server overloaded
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 2)));
                Baskets[o] = false;
                Orders[Guid.NewGuid().ToString()] = o;
            }

            return new object();
        }
    }
}