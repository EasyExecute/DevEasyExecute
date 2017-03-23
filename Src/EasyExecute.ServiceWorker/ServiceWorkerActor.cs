using Akka.Actor;
using EasyExecute.Messages;
using System;
using System.Threading.Tasks;

namespace EasyExecute.ServiceWorker
{
    public class ServiceWorkerActor : ReceiveActor
    {
        public ServiceWorkerActor(IActorRef parent,IActorRef executionQueryActorRef)
        {
            Receive<SetWorkMessage>(message =>
            {
                Execute(parent, message, Sender, executionQueryActorRef);
            });
        }

        private void Execute(IActorRef parent, SetWorkMessage message, IActorRef sender,IActorRef executionQueryActorRef)
        {
            
            var messageClosure = message;
            var senderClosure = sender;
            var parentClosure = parent;
            var executionQueryActorRefClosure = executionQueryActorRef;
            IEasyExecuteResponseMessage resultMessage;
            executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, "worker about to start working"+" - retry count: " + messageClosure.WorkFactory.MaxRetryCount));
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
                                executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, "execution result is faulted for work "+messageClosure.Id));
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
                                    executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, "operation completed but client said its was a failed operation for work " + messageClosure.Id));

                                }
                                else
                                {
                                    resultMessage = new SetWorkSucceededMessage(result, messageClosure.Id);
                                    executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, "operation completed successfully for work " + messageClosure.Id));

                                }
                            }

                            if (!(resultMessage is SetWorkSucceededMessage) && workFactory.MaxRetryCount > 0)
                            {
                                messageClosure.WorkFactory.MaxRetryCount--;
                                executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, "operation retrying for work " + messageClosure.Id+" retry count : " + messageClosure.WorkFactory.MaxRetryCount));
                                Execute(parentClosure, messageClosure, senderClosure, executionQueryActorRefClosure);
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