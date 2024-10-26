using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.System;
using Shuttle.Core.Threading;
using JsonSerializer = Shuttle.Core.Serialization.JsonSerializer;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class DeferredProcessingFixture
{
    [Test]
    public async Task Should_be_able_to_defer_processing()
    {
        var serializer = new JsonSerializer(Options.Create(new JsonSerializerOptions()));
        var pipelineFactory = new Mock<IPipelineFactory>();
        var configuration = new Mock<IServiceBusConfiguration>();

        var inboxConfiguration = new InboxConfiguration
        {
            WorkQueue = new MemoryQueue(new("memory://memory/work-queue")),
            DeferredQueue = new MemoryQueue(new("memory://memory/deferred-queue")),
            ErrorQueue = new MemoryQueue(new("memory://memory/error-queue"))
        };

        configuration.Setup(m => m.Inbox).Returns(inboxConfiguration);

        var processDeferredMessageObserver = new ProcessDeferredMessageObserver();

        pipelineFactory.Setup(m => m.GetPipeline<DeferredMessagePipeline>()).Returns(
            new DeferredMessagePipeline(
                configuration.Object,
                new GetDeferredMessageObserver(),
                new DeserializeTransportMessageObserver(Options.Create(new ServiceBusOptions()), serializer, new EnvironmentService(), new ProcessService()),
                processDeferredMessageObserver
            ));

        var deferredMessageProcessor = new DeferredMessageProcessor(Options.Create(new ServiceBusOptions { Inbox = new() { DeferredQueueUri = "memory://memory/deferred-queue", DeferredMessageProcessorResetInterval = TimeSpan.FromMilliseconds(500) } }), pipelineFactory.Object);

        deferredMessageProcessor.DeferredMessageProcessingHalted += (_, _) =>
        {
            Console.WriteLine(@"[deferred processing halted]");
        };

        var transportMessage1 = CreateTransportMessage(DateTime.Now.AddSeconds(3).ToUniversalTime());
        var transportMessage2 = CreateTransportMessage(DateTime.Now.AddSeconds(2).ToUniversalTime());
        var transportMessage3 = CreateTransportMessage(DateTime.Now.AddSeconds(1).ToUniversalTime());

        await inboxConfiguration.DeferredQueue.EnqueueAsync(transportMessage1, await serializer.SerializeAsync(transportMessage1));
        await inboxConfiguration.DeferredQueue.EnqueueAsync(transportMessage2, await serializer.SerializeAsync(transportMessage2));
        await inboxConfiguration.DeferredQueue.EnqueueAsync(transportMessage3, await serializer.SerializeAsync(transportMessage3));

        var messagesReturned = new List<TransportMessage>();

        processDeferredMessageObserver.MessageReturned += (_, e) =>
        {
            messagesReturned.Add(e.TransportMessage);
        };

        var timeout = DateTime.Now.AddMilliseconds(3500);

        await new ProcessorThreadPool("DeferredMessageProcessor", 1, new DeferredMessageProcessorFactory(deferredMessageProcessor), new()).StartAsync();

        while (messagesReturned.Count < 3 && DateTime.Now < timeout)
        {
            Thread.Sleep(250);
        }

        Assert.That(messagesReturned.Find(item => item.MessageId.Equals(transportMessage1.MessageId)), Is.Not.Null);
        Assert.That(messagesReturned.Find(item => item.MessageId.Equals(transportMessage2.MessageId)), Is.Not.Null);
        Assert.That(messagesReturned.Find(item => item.MessageId.Equals(transportMessage3.MessageId)), Is.Not.Null);
    }

    private static TransportMessage CreateTransportMessage(DateTime ignoreTillDate)
    {
        return new()
        {
            MessageId = new("973808b9-8cc6-433b-b9d2-a08e1236c104"),
            PrincipalIdentityName = "unit-test",
            MessageType = "message-type",
            AssemblyQualifiedName = "assembly-qualified-name",
            IgnoreTillDate = ignoreTillDate
        };
    }
}