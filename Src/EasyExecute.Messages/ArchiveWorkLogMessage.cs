namespace EasyExecute.Messages
{
    public class ArchiveWorkLogMessage : IEasyExecuteRequestMessage
    {
        public ArchiveWorkLogMessage(string workerId, string message)
        {
            WorkerId = workerId;
            Message = message;
        }

        public string WorkerId { get; private set; }
        public string Message { get; private set; }
    }
}