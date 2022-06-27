using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class DeferredProcessingFixture
    {
        [Test]
        public void Should_be_able_to_defer_processing()
        {
            var pipelineFactory = new Mock<IPipelineFactory>();
            var configuration = new Mock<IServiceBusConfiguration>();

            configuration.Setup(m => m.Inbox).Returns(new InboxQueueConfiguration
            {
                DeferredMessageProcessor = new DeferredMessageProcessor(pipelineFactory.Object)
            });

            var getDeferredMessageObserver = new Mock<IGetDeferredMessageObserver>();
            var deserializeTransportMessageObserver = new Mock<IDeserializeTransportMessageObserver>();
            var processDeferredMessageObserver = new Mock<IProcessDeferredMessageObserver>();

            var index = 0;

            var transportMessage1 = new TransportMessage
            {
                MessageId = new Guid("973808b9-8cc6-433b-b9d2-a08e1236c104"),
                IgnoreTillDate = DateTime.Now.AddSeconds(5)
            };

            var transportMessage2 = new TransportMessage
            {
                MessageId = new Guid("d06bf8e5-f81c-4ca6-a3c4-368efae97b0b"),
                IgnoreTillDate = DateTime.Now.AddSeconds(4)
            };

            var transportMessage3 = new TransportMessage
            {
                MessageId = new Guid("b7d21a52-bb98-4a59-85cb-d1e1e26e72df"),
                IgnoreTillDate = DateTime.Now.AddSeconds(3)
            };

            var deferredMessages = new List<TransportMessage> {transportMessage1, transportMessage2, transportMessage3};

            getDeferredMessageObserver.Setup(m => m.Execute(It.IsAny<OnGetMessage>())).Callback(
                (OnGetMessage pipelineEvent) =>
                {
                    if (deferredMessages.Count == 0)
                    {
                        pipelineEvent.Pipeline.Abort();
                        return;
                    }

                    if (index > deferredMessages.Count - 1)
                    {
                        index = 0;
                    }

                    var transportMessage = deferredMessages[index];
                    var deferredMessageReturned = !transportMessage.IsIgnoring();

                    Console.WriteLine($@"[processing]: index = {index} / message id = {transportMessage.MessageId}");

                    pipelineEvent.Pipeline.State.SetTransportMessage(transportMessage);
                    pipelineEvent.Pipeline.State.SetWorking();
                    pipelineEvent.Pipeline.State.SetDeferredMessageReturned(deferredMessageReturned);

                    if (deferredMessageReturned)
                    {
                        deferredMessages.RemoveAt(index);

                        Console.WriteLine($@"[returned]: index = {index} / message id = {transportMessage.MessageId}");
                    }

                    index++;
                });

            pipelineFactory.Setup(m => m.GetPipeline<DeferredMessagePipeline>()).Returns(
                new DeferredMessagePipeline(
                    configuration.Object,
                    getDeferredMessageObserver.Object,
                    deserializeTransportMessageObserver.Object,
                    processDeferredMessageObserver.Object
                ));

            using (new ProcessorThreadPool("DeferredMessageProcessor", 1, TimeSpan.FromSeconds(1), 
                new DeferredMessageProcessorFactory(configuration.Object)).Start())
            {
                while (deferredMessages.Count > 0)
                {
                    Thread.Sleep(250);
                }
            }
        }
    }
}