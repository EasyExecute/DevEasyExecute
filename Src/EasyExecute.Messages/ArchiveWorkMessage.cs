namespace EasyExecute.Messages
{
    public class ArchiveWorkMessage : IEasyExecuteResponseMessage
    {
        public ArchiveWorkMessage( string workerId, Worker worker, string message)
        {
            WorkerId = workerId;
            Worker = worker;
            Message = message;
        }

        public string WorkerId { get; private set; }
        public Worker Worker { get; private set; }
        public string Message { get; private set; }
    }
}