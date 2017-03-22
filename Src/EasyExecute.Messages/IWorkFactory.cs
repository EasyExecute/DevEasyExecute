using System.Threading.Tasks;

namespace EasyExecute.Messages
{
    public interface IWorkFactory
    {
        bool RunAsyncMethod { set; get; }
        int MaxRetryCount { get; set; }

        object Execute();

        Task<object> ExecuteAsync(object command);

        bool IsAFailedResult(object result);
    }
}