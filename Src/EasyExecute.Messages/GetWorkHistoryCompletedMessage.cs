using System;
using System.Collections.Generic;

namespace EasyExecute.Messages
{
    public class GetWorkHistoryCompletedMessage : IEasyExecuteResponseMessage
    {
        public GetWorkHistoryCompletedMessage(List<Worker> workHistory, DateTime lastSystemAccessedTime)
        {
            WorkHistory = workHistory;
            LastSystemAccessedTime = lastSystemAccessedTime;
        }

        public List<Worker> WorkHistory { get; private set; }
        public DateTime LastSystemAccessedTime { get; private set; }
        public string WorkerId { get; set; }
    }
}