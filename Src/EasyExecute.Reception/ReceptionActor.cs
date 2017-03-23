using Akka.Actor;
using Akka.Routing;
using EasyExecute.Messages;
using EasyExecute.ServiceWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using EasyExecute.ExecutionQuery;

namespace EasyExecute.Reception
{
    public class ReceptionActor : ReceiveActor
    {
        private DateTime LastAccessedTime { set; get; }
        private TimeSpan PurgeInterval { set; get; }
        private Action<Worker> OnWorkersPurged { set; get; }

        public ReceptionActor(TimeSpan purgeInterval, Action<Worker> onWorkersPurged,IActorRef executionQueryActorRef)
        {
            OnWorkersPurged = onWorkersPurged;
            PurgeInterval = purgeInterval;
            ServiceWorkerStore = new Dictionary<string, Worker>();

            ExecutionQueryActorRef = executionQueryActorRef;
            var serviceWorkerActorRef =
                Context.ActorOf(Props.Create(() => new ServiceWorkerActor(Self, ExecutionQueryActorRef)).WithRouter(new RoundRobinPool(10)));

            Receive<GetWorkHistoryMessage>(message =>
            {
                LogSteps("request to get work history", message.WorkId);
                Sender.Tell(string.IsNullOrEmpty(message.WorkId)
                    ? new GetWorkHistoryCompletedMessage(ServiceWorkerStore.Select(x => x.Value).ToList(), LastAccessedTime)
                    : new GetWorkHistoryCompletedMessage(
                        ServiceWorkerStore.Where(x => x.Key == message.WorkId).Select(x => x.Value).ToList(), LastAccessedTime));
            });

            Receive<SetWorkMessage>(message =>
            {
                LogSteps("request to do work " + message.Id, message.Id);
                LastAccessedTime = DateTime.UtcNow;
                if (ServiceWorkerStore.ContainsKey(message.Id))
                {
                    var m = $"Duplicate work ID: {message.Id} at {LastAccessedTime}";
                    Sender.Tell(new SetCompleteWorkErrorMessage(m, message.Id, ServiceWorkerStore[message.Id].Result, true));
                    LogSteps(m, message.Id);
                }
                else if (string.IsNullOrEmpty(message.Id))
                {
                    var m = $"Null or empty ID: {message.Id} at {LastAccessedTime}";
                    Sender.Tell(new SetWorkErrorMessage(m, message.Id, null));
                    LogSteps(m, message.Id);
                }
                else
                {
                    LogSteps($"sending work {message.Id} off to be done ...", message.Id);
                    ServiceWorkerStore.Add(message.Id, new Worker(message.Id, new WorkerStatus
                    {
                        CreatedDateTime = DateTime.UtcNow
                    }, null, message.Command, message.StoreCommands, message.ExpiresAt));
                    serviceWorkerActorRef.Forward(message);
                }
            });
            Receive<SetWorkErrorMessage>(message =>
            {
                LogSteps("Work execution failed", message.WorkerId);
                if (!ServiceWorkerStore.ContainsKey(message.WorkerId)) return;

                var work = ServiceWorkerStore[message.WorkerId];
                var worker = new Worker(message.WorkerId, new WorkerStatus
                {
                    CreatedDateTime = work.WorkerStatus.CreatedDateTime,
                    CompletedDateTime = DateTime.UtcNow,
                    IsCompleted = true,
                    Succeeded = false
                }, message.Result, work.StoreCommands ? work.Command : null, work.StoreCommands, work.ExpiresAt);

                ExecutionQueryActorRef.Tell(new ArchiveWorkMessage(message.WorkerId, worker));

                RemoveWorkerFromDictionary(message.WorkerId);
            });
            Receive<PurgeMessage>(_ =>
            {
                var workers = ServiceWorkerStore.Where(x => x.Value.ExpiresAt == null || x.Value.ExpiresAt <= DateTime.UtcNow).Select(x => x.Key).ToList();
                LogSteps("purging workers count : " + workers.Count);
                workers.ForEach(RemoveWorkerFromDictionary);
            });
            Receive<SetWorkSucceededMessage>(message =>
            {
                LogSteps("Work execution succeeded", message.WorkerId);
                if (!ServiceWorkerStore.ContainsKey(message.WorkerId)) return;

                var work = ServiceWorkerStore[message.WorkerId];
                ServiceWorkerStore.Remove(message.WorkerId);
                var worker = new Worker(message.WorkerId, new WorkerStatus
                {
                    CreatedDateTime = work.WorkerStatus.CreatedDateTime,
                    CompletedDateTime = DateTime.UtcNow,
                    IsCompleted = true,
                    Succeeded = true
                }, message.Result, work.StoreCommands ? work.Command : null, work.StoreCommands, work.ExpiresAt);
                ServiceWorkerStore.Add(message.WorkerId, worker);

                ExecutionQueryActorRef.Tell(new ArchiveWorkMessage(message.WorkerId, worker));

            });
            Context.System.Scheduler.ScheduleTellRepeatedly(PurgeInterval, PurgeInterval, Self, new PurgeMessage(), Self);
        }

        public IActorRef ExecutionQueryActorRef { get; set; }

        private void LogSteps(string message, string WorkerId = nameof(ReceptionActor))
        {
            ExecutionQueryActorRef.Tell(new ArchiveWorkLogMessage(WorkerId, message));
        }

        private Dictionary<string, Worker> ServiceWorkerStore { get; }

        private void RemoveWorkerFromDictionary(string workerId)
        {
            LogSteps("removing work ", workerId);
            if (!ServiceWorkerStore.ContainsKey(workerId)) return;

            var worker = ServiceWorkerStore[workerId];
            ServiceWorkerStore.Remove(workerId);
            try
            {
                OnWorkersPurged?.Invoke(worker);
            }
            catch (Exception e)
            {
                LogSteps("error executing OnWorkersPurged ", workerId);
                //todo how to handle?
            }
        }
    }
}