using System;

namespace EasyExecute.Common
{
    public class ExecutionRequestOptions
    {
        public bool? ReturnExistingResultWhenDuplicateId { set; get; }
        public bool StoreCommands { set; get; }
        public TimeSpan? CacheExpirationPeriod { set; get; }
        public int MaxRetryCount { set; get; }
        public bool ExecuteReactively { set; get; }
    }
}