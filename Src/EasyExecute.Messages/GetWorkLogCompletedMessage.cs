using System.Collections.Generic;

namespace EasyExecute.Messages
{
    public class GetWorkLogCompletedMessage : IEasyExecuteResponseMessage
    {
        public GetWorkLogCompletedMessage(List<Worker> workHistory, List<string> workLog)
        {
            WorkHistory = workHistory;
            WorkLog = workLog;
        }

        public List<Worker> WorkHistory { get; private set; }
        public List<string> WorkLog { get; private set; }
        public string WorkerId { get; set; }
    }
}