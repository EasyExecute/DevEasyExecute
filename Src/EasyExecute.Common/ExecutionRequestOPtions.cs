using EasyExecute.Messages;
using System;

namespace EasyExecute.Common
{
    public class ExecutionRequestOptions
    {
        public bool DontCacheResultById { set; get; }
        public bool StoreCommands { set; get; }
        public TimeSpan? CacheExpirationPeriod { set; get; }
        public int MaxRetryCount { set; get; }
        public bool ExecuteReactively { set; get; }
        public Action<Worker> OnWorkerPurged { set; get; }
        public bool FailExecutionIfTaskIsCancelled { get; set; }
    }
}