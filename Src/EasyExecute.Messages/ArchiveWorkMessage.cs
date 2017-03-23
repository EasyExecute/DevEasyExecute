namespace EasyExecute.Messages
{
    public class ArchiveWorkMessage : IEasyExecuteRequestMessage
    {
        public ArchiveWorkMessage( string workerId, Worker worker)
        {
            WorkerId = workerId;
            Worker = worker;
        }

        public string WorkerId { get; private set; }
        public Worker Worker { get; private set; }
    }
}