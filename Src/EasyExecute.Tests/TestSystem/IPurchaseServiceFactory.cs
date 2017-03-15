using System.Threading.Tasks;

namespace EasyExecute.Tests.TestSystem
{
    public interface IPurchaseServiceFactory
    {
        Task<object> RunPurchaseServiceAsync(string o);
    }
}