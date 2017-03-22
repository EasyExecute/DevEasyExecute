using System;

namespace EasyExecute.Messages
{
    public class WorkerStatus
    {
        public DateTime CreatedDateTime { get; set; }
        public DateTime CompletedDateTime { get; set; }
        public bool IsCompleted { get; set; }
        public bool Succeeded { get; set; }
    }
}