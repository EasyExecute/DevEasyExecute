namespace EasyExecute.Messages
{
    public class SetWorkCompletedMessage : IEasyExecuteResponseMessage
    {
        public SetWorkCompletedMessage(object result, string id, string executionId)
        {
            Result = result;
            Id = id;
            WorkerId = executionId;
        }

        public object Result { get; private set; }
        public string Id { get; private set; }
        public string WorkerId { get; private set; }
    }
}