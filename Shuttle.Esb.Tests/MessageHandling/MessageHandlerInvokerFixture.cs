using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Tests.MessageHandling;

[TestFixture]
public class MessageHandlerInvokerFixture
{
    [Test]
    public void Should_be_able_to_invoke()
    {
        Should_be_able_to_invoke_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_invoke_async()
    {
        await Should_be_able_to_invoke_async(false);
    }

    private async Task Should_be_able_to_invoke_async(bool sync)
    {
        var services = new ServiceCollection();

        services.AddSingleton(typeof(IMessageHandler<>).MakeGenericType(typeof(WorkMessage)), typeof(WorkHandler));
        services.AddSingleton(typeof(IAsyncMessageHandler<>).MakeGenericType(typeof(WorkMessage)), typeof(AsyncWorkHandler));

        var serviceProvider = services.BuildServiceProvider();

        var invoker = new MessageHandlerInvoker(serviceProvider, new Mock<IMessageSender>().Object);

        var onHandleMessage = new OnHandleMessage();
        var transportMessage = new TransportMessage
        {
            Message = sync ? Stream.Null.ToBytes() : await Stream.Null.ToBytesAsync()
        };

        onHandleMessage.Reset(new Pipeline());

        onHandleMessage.Pipeline.State.Add(StateKeys.Message, new WorkMessage());
        onHandleMessage.Pipeline.State.Add(StateKeys.TransportMessage, transportMessage);

        var result = sync 
            ? invoker.Invoke(onHandleMessage)
            : await invoker.InvokeAsync(onHandleMessage);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Invoked, Is.True);

    }
}