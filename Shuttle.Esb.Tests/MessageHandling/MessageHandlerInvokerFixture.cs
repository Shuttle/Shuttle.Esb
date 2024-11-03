using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;

namespace Shuttle.Esb.Tests.MessageHandling;

[TestFixture]
public class MessageHandlerInvokerFixture
{
    [Test]
    public async Task Should_be_able_to_invoke_async()
    {
        var services = new ServiceCollection();

        services.AddSingleton(typeof(IMessageHandler<>).MakeGenericType(typeof(WorkMessage)), typeof(WorkHandler));

        var serviceProvider = services.BuildServiceProvider();

        var invoker = new MessageHandlerInvoker(serviceProvider, new Mock<IMessageSender>().Object);

        var transportMessage = new TransportMessage
        {
            Message = await Stream.Null.ToBytesAsync()
        };

        var pipelineContext = new PipelineContext<OnHandleMessage>(new Pipeline(new Mock<IServiceProvider>().Object));

        pipelineContext.Pipeline.State.Add(StateKeys.Message, new WorkMessage());
        pipelineContext.Pipeline.State.Add(StateKeys.TransportMessage, transportMessage);

        var result = await invoker.InvokeAsync(pipelineContext);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Invoked, Is.True);
    }
}