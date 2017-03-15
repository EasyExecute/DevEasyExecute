namespace EasyExecute.Messages
{
    public class Worker
    {
        public Worker(string workerId, WorkerStatus workerStatus, object result, object command, bool storeCommands)
        {
            WorkerStatus = workerStatus;
            Result = result;
            Command = command;
            WorkerId = workerId;
            StoreCommands = storeCommands;
        }

        public WorkerStatus WorkerStatus { get; private set; }

        public string WorkerId { get; private set; }

        public object Result { get; private set; }
        public object Command { get; private set; }
        public bool StoreCommands { get; private set; }
    }
}