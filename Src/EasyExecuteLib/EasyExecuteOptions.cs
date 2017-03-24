using Akka.Actor;
using EasyExecute.Messages;
using System;

namespace EasyExecuteLib
{
    public class EasyExecuteOptions
    {
        public TimeSpan? maxExecutionTimePerAskCall { set; get; }
        public string serverActorSystemName { set; get; }
        public ActorSystem actorSystem { set; get; }
        public string actorSystemConfig { set; get; }
        public TimeSpan? purgeInterval { set; get; }
        public Action<Worker> onWorkerPurged { set; get; }
    }
}