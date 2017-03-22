using Akka.Actor;
using EasyExecute.Messages;
using System;
using System.Threading.Tasks;

namespace EasyExecute.ServiceWorker
{
    public class ServiceWorkerActor : ReceiveActor
    {
        public ServiceWorkerActor(IActorRef parent)
        {
            Receive<SetWorkMessage>(message =>
            {
                Execute(parent, message, Sender);
            });
        }

        private void Execute(IActorRef parent, SetWorkMessage message, IActorRef sender)
        {
            var messageClosure = message;
            var senderClosure = sender;
            var parentClosure = parent;
            IEasyExecuteResponseMessage resultMessage;
            try
            {
                var workFactory = messageClosure.WorkFactory;
                if (workFactory.RunAsyncMethod)
                {
                    workFactory.ExecuteAsync(messageClosure.Command)
                        .ContinueWith(r =>
                        {
                            if (r.IsFaulted)
                            {
                                resultMessage = new SetWorkErrorMessage("Unable to complete operation", messageClosure.Id,
                                    null);
                            }
                            else
                            {
                                var result = r.Result;
                                if (workFactory.IsAFailedResult(result))
                                {
                                    resultMessage =
                                        new SetWorkErrorMessage(
                                            "operation completed but client said its was a failed operation",
                                            messageClosure.Id, result);
                                }
                                else
                                {
                                    resultMessage = new SetWorkSucceededMessage(result, messageClosure.Id);
                                }
                            }

                            if (!(resultMessage is SetWorkSucceededMessage) && workFactory.MaxRetryCount > 0)
                            {
                                messageClosure.WorkFactory.MaxRetryCount--;
                                Execute(parentClosure, messageClosure, senderClosure);
                            }
                            else
                            {
                                parentClosure.Tell(resultMessage);
                                senderClosure.Tell(resultMessage);
                            }

                            return resultMessage;
                        },
                            TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously);
                }
                else
                {
                    workFactory.Execute();
                }
            }
            catch (Exception e)
            {
                resultMessage = new SetWorkErrorMessage(e.Message + " " + e.InnerException?.Message, messageClosure.Id, null);
                senderClosure.Tell(resultMessage);
                Context.Parent.Tell(resultMessage);
            }
        }
    }
}