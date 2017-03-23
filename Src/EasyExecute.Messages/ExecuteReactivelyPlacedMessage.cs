namespace EasyExecute.Messages
{
    public class ExecuteReactivelyPlacedMessage : IEasyExecuteResponseMessage
    {
        public ExecuteReactivelyPlacedMessage(string workerId)
        {
            WorkerId = workerId;
        }

        public string WorkerId { get; private set; }
    }
}