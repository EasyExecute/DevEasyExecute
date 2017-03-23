namespace EasyExecute.Messages
{
    public class ArchiveWorkLogCompletedMessage : IEasyExecuteResponseMessage
    {
        public ArchiveWorkLogCompletedMessage(string workerId)
        {
            WorkerId = workerId;
        }

        public string WorkerId { get; private set; }
    }
}