namespace EasyExecute.Messages
{
    public class SetWorkCompletedMessage : IEasyExecuteResponseMessage
    {
        public SetWorkCompletedMessage(object result, string id)
        {
            Result = result;
            Id = id;
        }

        public object Result { get; private set; }
        public string Id { get; private set; }
    }
}