using System.Threading.Tasks;

namespace EasyExecute.Tests.TestSystem
{
    public class NullPurchaseServiceFactory : IPurchaseServiceFactory
    {
        public Task<object> RunPurchaseServiceAsync(string o)
        {
            return Task.FromResult(new object());
        }
    }
}