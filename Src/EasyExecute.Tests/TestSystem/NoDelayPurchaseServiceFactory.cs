using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EasyExecute.Tests.TestSystem
{
    public class NoDelayPurchaseServiceFactory : IPurchaseServiceFactory
    {
        public NoDelayPurchaseServiceFactory(ConcurrentDictionary<string, bool> baskets,
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
                Baskets[o] = false;
                Orders[Guid.NewGuid().ToString()] = o;
            }

            return new object();
        }
    }
}