namespace EasyExecute.Messages
{
    public class GetWorkHistoryMessage : IEasyExecuteRequestMessage
    {
        public string WorkId { get; private set; }

        public GetWorkHistoryMessage(string workId)
        {
            WorkId = workId;
        }
    }
}