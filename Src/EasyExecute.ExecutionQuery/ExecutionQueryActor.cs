using Akka.Actor;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using EasyExecute.Messages;

namespace EasyExecute.ExecutionQuery
{
    public class ExecutionQueryActor : ReceiveActor
    {
        private Dictionary<string, List<Worker>> ArchiveServiceWorkerStore { get; }
        public ExecutionQueryActor()
        {
            ArchiveServiceWorkerStore=new Dictionary<string, List<Worker>>();
            Receive<ArchiveWorkMessage>(message =>
            {
                if (!ArchiveServiceWorkerStore.ContainsKey(message.WorkerId))
                {
                    ArchiveServiceWorkerStore.Add(message.WorkerId, new List<Worker>());
                }
                ArchiveServiceWorkerStore[message.WorkerId].Add(message.Worker);
            });

        }
    }
}