using Akka.Actor;
using EasyExecute.Messages;
using System;
using System.Threading.Tasks;

namespace EasyExecute.ServiceWorker
{
    public class ServiceWorkerActor : ReceiveActor
    {
        public ServiceWorkerActor( IActorRef parent)
        {
            Receive<SetWorkMessage>(message =>
            {
                var senderClosure = Sender;
                var parentClosure = parent;// todo probably dont need to close over parent
                IEasyExecuteResponseMessage resultMessage;
                try
                {
                    var workFactory = message.WorkFactory;
                    if (workFactory.RunAsyncMethod)
                    {
                        workFactory.ExecuteAsync(message.Command)
                            .ContinueWith(r =>
                            {
                                if (r.IsFaulted)
                                {
                                    resultMessage = new SetWorkErrorMessage("Unable to complete operation", message.Id,null);
                               }
                                else
                                {
                                    var result = r.Result;
                                    if (workFactory.IsAFailedResult(result))
                                    {
                                        resultMessage = new SetWorkErrorMessage("operation completed but client said its was a failed operation", message.Id,result);
                                    }
                                    else
                                    {
                                        resultMessage = new SetWorkSucceededMessage(result, message.Id);
                                    }
                                }
                                parentClosure.Tell(resultMessage);// because  There is no active ActorContext, this is most likely due to use of async operations from within this actor.
                                senderClosure.Tell(resultMessage);
                                return resultMessage;
                            },
                                TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                       ;// .PipeTo(senderClosure).PipeTo(parentClosure);
                    }
                    else
                    {
                        workFactory.Execute();
                    }
                }
                catch (Exception e)
                {
                    resultMessage = new SetWorkErrorMessage(e.Message + " " + e.InnerException?.Message, message.Id,null);
                    senderClosure.Tell(resultMessage);
                    Context.Parent.Tell(resultMessage);
                }
            });
        }
    }
}