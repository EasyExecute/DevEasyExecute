using Akka.Actor;
using EasyExecute.Common;
using EasyExecute.Messages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyExecute.ServiceWorker
{
    public class ServiceWorkerActor : ReceiveActor
    {
        public ServiceWorkerActor(IActorRef parent, IActorRef executionQueryActorRef)
        {
            Receive<SetWorkMessage>(message =>
            {
                Execute(parent, message, Sender, executionQueryActorRef);
            });
        }

        private void Execute(IActorRef parent, SetWorkMessage message, IActorRef sender, IActorRef executionQueryActorRef)
        {
            var messageClosure = message;
            var senderClosure = sender;
            var parentClosure = parent;
            var executionQueryActorRefClosure = executionQueryActorRef;
            IEasyExecuteResponseMessage resultMessage;
            executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, "worker about to start working" + " - retry count: " + messageClosure.WorkFactory.MaxRetryCount));
            try
            {
                var workFactory = messageClosure.WorkFactory;
                if (workFactory.RunAsyncMethod)
                {
                    var workTask= workFactory.ExecuteAsync(messageClosure.Command);
                    workTask.ConfigureAwait(false);
                    workTask.ContinueWith(r =>
                        {
                            if (r.IsFaulted)
                            {
                                var exceptionThrown = "";
                                if (r.Exception != null)
                                {
                                    exceptionThrown = r.Exception.Flatten().InnerExceptions.Aggregate(exceptionThrown, (current, exception) => current + string.Join("<br>\n\r", exception.GetMessages()));
                                }
                                var excMsg = $"Unable to complete operation {messageClosure.Id} : {exceptionThrown}";
                                resultMessage = new SetWorkErrorMessage(excMsg, messageClosure.Id, null);
                                executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, excMsg));
                            }

                            else
                            {
                                if (r.IsCanceled && messageClosure.FailExecutionIfTaskIsCancelled)
                                {
                                    var excMsg = $"Unable to complete operation {messageClosure.Id} : Ececution is set to fail if task is cancelled";
                                    resultMessage = new SetWorkErrorMessage(excMsg, messageClosure.Id, null);
                                    executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, excMsg));
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

                            }


                            if (!(resultMessage is SetWorkSucceededMessage) && workFactory.MaxRetryCount > 0)
                            {
                                messageClosure.WorkFactory.MaxRetryCount--;
                                executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, "operation retrying for work " + messageClosure.Id + " retry count : " + messageClosure.WorkFactory.MaxRetryCount));
                                Execute(parentClosure, messageClosure, senderClosure, executionQueryActorRefClosure);
                            }
                            else
                            {
                                parentClosure.Tell(resultMessage);
                                senderClosure.Tell(resultMessage);
                            }

                            return resultMessage;
                        }
                        , TaskContinuationOptions.AttachedToParent &
                         TaskContinuationOptions.ExecuteSynchronously //&
                         //TaskContinuationOptions.LongRunning & dont start new thread
                         //TaskContinuationOptions.PreferFairness
                         );
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
        
        private void ExecuteAsTopLevelInvocation(IActorRef parent, SetWorkMessage message, IActorRef sender, IActorRef executionQueryActorRef)
        {
            var messageClosure = message;
            var senderClosure = sender;
            var parentClosure = parent;
            var executionQueryActorRefClosure = executionQueryActorRef;
            IEasyExecuteResponseMessage resultMessage;
            executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, "worker about to start working" + " - retry count: " + messageClosure.WorkFactory.MaxRetryCount));
            try
            {
                var workFactory = messageClosure.WorkFactory;
                if (workFactory.RunAsyncMethod)
                {
                    Exception exception = null;
                    object result = null;
                    try
                    {
                        result = Task.Run(() => workFactory.ExecuteAsync(messageClosure.Command)).Result;
                    }
                    catch (AggregateException ae)
                    {
                        exception = ae;
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }

                    if (exception == null)
                    {
                        if (workFactory.IsAFailedResult(result))
                        {
                            resultMessage =
                                new SetWorkErrorMessage(
                                    "operation completed but client said its was a failed operation",
                                    messageClosure.Id, result);
                            executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id,
                                "operation completed but client said its was a failed operation for work " +
                                messageClosure.Id));
                        }
                        else
                        {
                            resultMessage = new SetWorkSucceededMessage(result, messageClosure.Id);
                            executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id,
                                "operation completed successfully for work " + messageClosure.Id));
                        }
                    }
                    else
                    {
                        resultMessage = new SetWorkErrorMessage("Unable to complete operation", messageClosure.Id, null);
                        executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id,
                            "execution result is faulted for work " + messageClosure.Id));
                    }
                    if (!(resultMessage is SetWorkSucceededMessage) && workFactory.MaxRetryCount > 0)
                    {
                        messageClosure.WorkFactory.MaxRetryCount--;
                        executionQueryActorRefClosure.Tell(new ArchiveWorkLogMessage(message.Id, "operation retrying for work " + messageClosure.Id + " retry count : " + messageClosure.WorkFactory.MaxRetryCount));
                        ExecuteAsTopLevelInvocation(parentClosure, messageClosure, senderClosure, executionQueryActorRefClosure);
                    }
                    else
                    {
                        parentClosure.Tell(resultMessage);
                        senderClosure.Tell(resultMessage);
                    }

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