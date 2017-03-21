using System;

namespace EasyExecute.Common
{
    public class ExecutionRequestOptions 
    {
        public bool ReturnExistingResultWhenDuplicateId { set; get; }
        public bool StoreCommands { set; get; }
        public DateTime? PurgeAt { set; get; }
    }
}