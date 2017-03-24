using Akka.Actor;
using EasyExecute.Messages;
using System.Collections.Generic;
using System.Linq;

namespace EasyExecute.ExecutionQuery
{
    public class ExecutionQueryActor : ReceiveActor
    {
        private Dictionary<string, List<Worker>> ArchiveServiceWorkerStore { get; }

        public Dictionary<string, List<string>> ArchiveServiceWorkerLog { get; set; }

        public ExecutionQueryActor()
        {
            ArchiveServiceWorkerStore = new Dictionary<string, List<Worker>>();
            ArchiveServiceWorkerLog = new Dictionary<string, List<string>>();
            Receive<ArchiveWorkMessage>(message =>
            {
                if (!ArchiveServiceWorkerStore.ContainsKey(message.WorkerId))
                {
                    ArchiveServiceWorkerStore.Add(message.WorkerId, new List<Worker>());
                }
                ArchiveServiceWorkerStore[message.WorkerId].Add(message.Worker);
                Sender.Tell(new ArchiveWorkCompletedMessage(message.WorkerId));
            });
            Receive<ArchiveWorkLogMessage>(message =>
            {
                if (!ArchiveServiceWorkerLog.ContainsKey(message.WorkerId))
                {
                    ArchiveServiceWorkerLog.Add(message.WorkerId, new List<string>());
                }

                ArchiveServiceWorkerLog[message.WorkerId].Add(message.Message);
                Sender.Tell(new ArchiveWorkLogCompletedMessage(message.WorkerId));
            });
            Receive<GetWorkLogMessage>(message =>
            {
                Sender.Tell(string.IsNullOrEmpty(message.WorkId)
                    ? new GetWorkLogCompletedMessage(ArchiveServiceWorkerStore.SelectMany(x => x.Value).ToList(), ArchiveServiceWorkerLog.SelectMany(x => x.Value).ToList())
                    : new GetWorkLogCompletedMessage(ArchiveServiceWorkerStore[message.WorkId], ArchiveServiceWorkerLog[message.WorkId]));
            });
        }
    }
}