using System.Collections.Generic;

namespace EasyExecute.Messages
{
    public class GetWorkLogCompletedMessage : IEasyExecuteResponseMessage
    {
        public GetWorkLogCompletedMessage(List<Worker> workHistory)
        {
            WorkHistory = workHistory;
        }

        public List<Worker> WorkHistory { get; private set; }
        public string WorkerId { get; set; }
    }
}