using Akka.Actor;
using System;

namespace EasyExecute.ActorSystemFactory
{
    public class ActorSystemCreator
    {
        public ActorSystem ServiceActorSystem { get; set; }

        public void CreateOrSetUpActorSystem(string serverActorSystemName = null, ActorSystem actorSystem = null,
            string actorSystemConfig = null)
        {
            ServiceActorSystem = string.IsNullOrEmpty(serverActorSystemName)
                ? actorSystem
                : (string.IsNullOrEmpty(actorSystemConfig)
                    ? ActorSystem.Create(serverActorSystemName)
                    : ActorSystem.Create(serverActorSystemName, actorSystemConfig));

            if (ServiceActorSystem != null) return;
            const string message = "Invalid ActorSystemName.Please set up 'ServerActorSystemName' in the config file";

            throw new Exception(message);
        }

        public void TerminateActorSystem()
        {
            ServiceActorSystem?.Terminate().Wait();
            ServiceActorSystem?.Dispose();
        }
    }
}