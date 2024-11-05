using System;
using System.Collections.Generic;
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
    public async Task Should_be_able_to_invoke_message_handler_async()
    {
        var services = new ServiceCollection();

        services.AddSingleton(typeof(IMessageHandler<>).MakeGenericType(typeof(WorkMessage)), typeof(WorkHandler));

        var serviceProvider = services.BuildServiceProvider();

        var invoker = new MessageHandlerInvoker(serviceProvider, new Mock<IMessageSender>().Object, new MessageHandlerDelegateProvider(new Dictionary<Type, MessageHandlerDelegate>()));

        var transportMessage = new TransportMessage
        {
            Message = await Stream.Null.ToBytesAsync()
        };

        var pipelineContext = new PipelineContext<OnHandleMessage>(new Pipeline(new Mock<IServiceProvider>().Object));

        pipelineContext.Pipeline.State.Add(StateKeys.Message, new WorkMessage());
        pipelineContext.Pipeline.State.Add(StateKeys.TransportMessage, transportMessage);

        Assert.That(await invoker.InvokeAsync(pipelineContext), Is.True);
    }
    
    [Test]
    public async Task Should_be_able_to_invoke_delegate_async()
    {
        var services = new ServiceCollection();

        var builder = new ServiceBusBuilder(services)
            .MapMessageHandler(async (IHandlerContext<WorkMessage> context) =>
            {
                Console.WriteLine($@"[work-message] : guid = {context.Message.Guid}");

                await Task.CompletedTask;
            });

        var serviceProvider = services.BuildServiceProvider();

        var invoker = new MessageHandlerInvoker(serviceProvider, new Mock<IMessageSender>().Object, new MessageHandlerDelegateProvider(builder.GetDelegates()));

        var transportMessage = new TransportMessage
        {
            Message = await Stream.Null.ToBytesAsync()
        };

        var pipelineContext = new PipelineContext<OnHandleMessage>(new Pipeline(new Mock<IServiceProvider>().Object));

        pipelineContext.Pipeline.State.Add(StateKeys.Message, new WorkMessage());
        pipelineContext.Pipeline.State.Add(StateKeys.TransportMessage, transportMessage);

        Assert.That(await invoker.InvokeAsync(pipelineContext), Is.True);
    }
}