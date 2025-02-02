using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class AssembleMessageObserverFixture
{
    [Test]
    public async Task Should_be_able_to_assembly_transport_message_using_received_transport_message_async()
    {
        var serviceBusConfiguration = new Mock<IServiceBusConfiguration>();
        var observer = new AssembleMessageObserver(Options.Create(new ServiceBusOptions()), serviceBusConfiguration.Object, new DefaultIdentityProvider(Options.Create(new ServiceBusOptions())));

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object);
        var state = pipeline.State;

        state.SetMessage(new SimpleCommand());

        var transportMessageReceived = new TransportMessage
        {
            CorrelationId = Guid.NewGuid().ToString(),
            Headers = new()
            {
                new()
                {
                    Key = "key-one",
                    Value = "value-one"
                }
            }
        };

        state.SetTransportMessageReceived(transportMessageReceived);

        state.SetTransportMessageBuilder(builder =>
        {
            builder.Headers.Add(new()
            {
                Key = "key-two",
                Value = "value-two"
            });
        });

        var pipelineContext = new PipelineContext<OnAssembleMessage>(pipeline);
        
        await observer.ExecuteAsync(pipelineContext);

        var transportMessage = state.GetTransportMessage();

        Assert.That(transportMessage, Is.Not.Null);
        Assert.That(transportMessage!.Headers.Count, Is.EqualTo(2));
        Assert.That(transportMessage.Headers.Contains("key-one"), Is.True);
        Assert.That(transportMessage.Headers.Contains("key-two"), Is.True);
        Assert.That(transportMessage.Headers.Contains("key-three"), Is.False);
        Assert.That(transportMessage.MessageReceivedId, Is.EqualTo(transportMessageReceived.MessageId));
        Assert.That(transportMessage.CorrelationId, Is.EqualTo(transportMessageReceived.CorrelationId));
    }
}