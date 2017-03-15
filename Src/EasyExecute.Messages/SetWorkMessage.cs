namespace EasyExecute.Messages
{
    public class SetWorkMessage : IEasyExecuteRequestMessage
    {
        public SetWorkMessage(string id, object command, IWorkFactory workFactory, bool storeCommands)
        {
            Id = id;
            WorkFactory = workFactory;
            Command = command;
            StoreCommands = storeCommands;
        }

        public string Id { get; private set; }
        public IWorkFactory WorkFactory { private set; get; }
        public object Command { private set; get; }
        public bool StoreCommands { get; private set; }
    }
}