namespace EasyExecute.Messages
{
    public class ArchiveWorkCompletedMessage : IEasyExecuteResponseMessage
    {
        public ArchiveWorkCompletedMessage(string workerId)
        {
            WorkerId = workerId;
        }

        public string WorkerId { get; private set; }
    }
}