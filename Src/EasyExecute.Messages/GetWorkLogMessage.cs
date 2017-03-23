namespace EasyExecute.Messages
{
    public class GetWorkLogMessage : IEasyExecuteRequestMessage
    {
        public string WorkId { get; private set; }

        public GetWorkLogMessage(string workId)
        {
            WorkId = workId;
        }
    }
}