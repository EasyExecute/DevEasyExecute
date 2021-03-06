using System;

namespace EasyExecute.Messages
{
    public class SetWorkMessage : IEasyExecuteRequestMessage
    {
        public SetWorkMessage(string id, object command, IWorkFactory workFactory, bool storeCommands, DateTime? expiresAt, bool dontCacheResultById, Action<Worker> onWorkerPurged, bool failExecutionIfTaskIsCancelled)
        {
            Id = id;
            WorkFactory = workFactory;
            Command = command;
            StoreCommands = storeCommands;
            ExpiresAt = expiresAt;
            DontCacheResultById = dontCacheResultById;
            OnWorkerPurged = onWorkerPurged;
            FailExecutionIfTaskIsCancelled = failExecutionIfTaskIsCancelled;
        }

        public string Id { get; private set; }
        public IWorkFactory WorkFactory { private set; get; }
        public object Command { private set; get; }
        public bool StoreCommands { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public bool DontCacheResultById { private set; get; }
        public Action<Worker> OnWorkerPurged { set; get; }
        public bool FailExecutionIfTaskIsCancelled { get; private set; }
    }
}