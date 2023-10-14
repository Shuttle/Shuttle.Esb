using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class HandleMessageObserverFixture
{
    [Test]
    public void Should_be_able_to_return_when_no_message_handling_is_required()
    {
        Should_be_able_to_return_when_no_message_handling_is_required_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_return_when_no_message_handling_is_required_async()
    {
        await Should_be_able_to_return_when_no_message_handling_is_required_async(false);
    }

    private async Task Should_be_able_to_return_when_no_message_handling_is_required_async(bool sync)
    {
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();

        var observer = new HandleMessageObserver(Options.Create(new ServiceBusOptions()), messageHandlerInvoker.Object, serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnHandleMessage>();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Ignore);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();

        pipeline.State.SetProcessingStatus(ProcessingStatus.MessageHandled);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(new TransportMessage { ExpiryDate = DateTime.Now.AddDays(-1) });

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }
}