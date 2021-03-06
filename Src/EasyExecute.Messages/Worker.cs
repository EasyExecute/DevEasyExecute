using System;

namespace EasyExecute.Messages
{
    public class Worker
    {
        public Worker(string workerId, WorkerStatus workerStatus, object result, object command, bool storeCommands, DateTime? expiresAt, bool dontCacheResultById, Action<Worker> onWorkerPurged)
        {
            WorkerStatus = workerStatus;
            Result = result;
            Command = command;
            WorkerId = workerId;
            StoreCommands = storeCommands;
            ExpiresAt = expiresAt;
            DontCacheResultById = dontCacheResultById;
            OnWorkerPurged = onWorkerPurged;
        }

        public WorkerStatus WorkerStatus { get; private set; }
        public string WorkerId { get; private set; }
        public object Result { get; private set; }
        public object Command { get; private set; }
        public bool StoreCommands { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public bool DontCacheResultById { private set; get; }
        public Action<Worker> OnWorkerPurged { private set; get; }
    }
}