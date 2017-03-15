using Akka.Actor;
using Akka.Routing;
using EasyExecute.Messages;
using EasyExecute.ServiceWorker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyExecute.Reception
{
    public class ReceptionActor : ReceiveActor
    {
        private DateTime LastAccessedTime { set; get; }
        private TimeSpan PurgeInterval { set; get; }
        private Action<Worker> OnWorkersPurged { set; get; }

        public ReceptionActor(TimeSpan? purgeInterval, Action<Worker> onWorkersPurged)
        {
            OnWorkersPurged = onWorkersPurged;
            PurgeInterval = purgeInterval ?? TimeSpan.FromHours(1);
            ServiceWorkerStore = new Dictionary<string, Worker>();
            var serviceWorkerActorRef =
                Context.ActorOf(Props.Create(() => new ServiceWorkerActor(Self)).WithRouter(new RoundRobinPool(10)));

            Receive<GetWorkHistoryMessage>(message =>
            {
                Sender.Tell(string.IsNullOrEmpty(message.WorkId)
                    ? new GetWorkHistoryCompletedMessage(ServiceWorkerStore.Select(x => x.Value).ToList(), LastAccessedTime)
                    : new GetWorkHistoryCompletedMessage(
                        ServiceWorkerStore.Where(x => x.Key == message.WorkId).Select(x => x.Value).ToList(), LastAccessedTime));
            });

            Receive<SetWorkMessage>(message =>
            {
                LastAccessedTime = DateTime.UtcNow;
                if (ServiceWorkerStore.ContainsKey(message.Id))
                {
                    Sender.Tell(new SetCompleteWorkErrorMessage($"Duplicate work ID: {message.Id} at {LastAccessedTime}", message.Id, ServiceWorkerStore[message.Id].Result, true));
                }
                else if (string.IsNullOrEmpty(message.Id))
                {
                    Sender.Tell(new SetWorkErrorMessage($"Null or empty ID: {message.Id} at {LastAccessedTime}", message.Id));
                }
                else
                {
                    ServiceWorkerStore.Add(message.Id, new Worker(message.Id, new WorkerStatus
                    {
                        CreatedDateTime = DateTime.UtcNow
                    }, null,message.Command,message.StoreCommands));
                    serviceWorkerActorRef.Forward(message);
                }
            });
            Receive<SetWorkErrorMessage>(message =>
            {
                RemoveWorkerFromDictionary(message.WorkerId);
            });
            Receive<PurgeMessage>(_ =>
            {
                var workers = ServiceWorkerStore.Select(x => x.Key).ToList();
                workers.ForEach(RemoveWorkerFromDictionary);
            });
            Receive<SetWorkSucceededMessage>(message =>
            {
                if (!ServiceWorkerStore.ContainsKey(message.WorkerId)) return;
                var work = ServiceWorkerStore[message.WorkerId];
                ServiceWorkerStore.Remove(message.WorkerId);
                ServiceWorkerStore.Add(message.WorkerId, new Worker(message.WorkerId, new WorkerStatus
                {
                    CreatedDateTime = work.WorkerStatus.CreatedDateTime,
                    CompletedDateTime = DateTime.UtcNow,
                    IsCompleted = true
                }, message.Result, work.StoreCommands? work.Command:null,work.StoreCommands));
            });
            Context.System.Scheduler.ScheduleTellRepeatedly(PurgeInterval, PurgeInterval, Self, new PurgeMessage(), Self);
        }

        private Dictionary<string, Worker> ServiceWorkerStore { get; }

        private void RemoveWorkerFromDictionary(string workerId)
        {
            if (!ServiceWorkerStore.ContainsKey(workerId)) return;
            var worker = ServiceWorkerStore[workerId];
            ServiceWorkerStore.Remove(workerId);
            try
            {
                OnWorkersPurged?.Invoke(worker);
            }
            catch (Exception e)
            {
                //todo how to handle?
            }
        }
    }
}