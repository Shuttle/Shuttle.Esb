using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class AssembleMessageObserverFixture
{
    [Test]
    public void Should_be_able_to_assembly_transport_message_using_received_transport_message()
    {
        Should_be_able_to_assembly_transport_message_using_received_transport_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_assembly_transport_message_using_received_transport_message_async()
    {
        await Should_be_able_to_assembly_transport_message_using_received_transport_message_async(false);
    }

    private async Task Should_be_able_to_assembly_transport_message_using_received_transport_message_async(bool sync)
    {
        var serviceBusConfiguration = new Mock<IServiceBusConfiguration>();
        var identityProvider = new Mock<IIdentityProvider>();

        var observer = new AssembleMessageObserver(Microsoft.Extensions.Options.Options.Create(new ServiceBusOptions()),
            serviceBusConfiguration.Object, identityProvider.Object);

        var pipelineEvent = new OnAssembleMessage();

        var pipeline = new Pipeline();
        var state = pipeline.State;

        state.SetMessage(new SimpleCommand());

        var transportMessageReceived = new TransportMessage
        {
            CorrelationId = Guid.NewGuid().ToString(),
            Headers = new List<TransportHeader>
            {
                new TransportHeader
                {
                    Key = "key-one",
                    Value = "value-one"
                }
            }
        };

        state.SetTransportMessageReceived(transportMessageReceived);
        
        state.SetTransportMessageBuilder(builder =>
        {
            builder.Headers.Add(new TransportHeader
            {
                Key = "key-two",
                Value = "value-two"
            });
        });

        pipelineEvent.Reset(pipeline);

        if (sync)
        {
            observer.Execute(pipelineEvent);
        }
        else
        {
            await observer.ExecuteAsync(pipelineEvent);
        }

        var transportMessage = state.GetTransportMessage();

        Assert.That(transportMessage, Is.Not.Null);
        Assert.That(transportMessage.Headers.Count, Is.EqualTo(2));
        Assert.That(transportMessage.Headers.Contains("key-one"), Is.True);
        Assert.That(transportMessage.Headers.Contains("key-two"), Is.True);
        Assert.That(transportMessage.Headers.Contains("key-three"), Is.False);
        Assert.That(transportMessage.MessageReceivedId, Is.EqualTo(transportMessageReceived.MessageId));
        Assert.That(transportMessage.CorrelationId, Is.EqualTo(transportMessageReceived.CorrelationId));
    }
}