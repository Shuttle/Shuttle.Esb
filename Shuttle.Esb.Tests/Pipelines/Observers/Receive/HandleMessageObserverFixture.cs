using System;
using System.IO;
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
    public async Task Should_be_able_to_return_when_no_message_handling_is_required_async()
    {
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();

        var observer = new HandleMessageObserver(Options.Create(new ServiceBusOptions()), messageHandlerInvoker.Object, serializer.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnHandleMessage>();

        pipeline.State.SetTransportMessage(new() { ExpiryDate = DateTime.Now.AddDays(-1) });

        await pipeline.ExecuteAsync();

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_execute_successful_invoke_async()
    {
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();

        var observer = new HandleMessageObserver(Options.Create(new ServiceBusOptions()), messageHandlerInvoker.Object, serializer.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnHandleMessage>();

        var transportMessage = new TransportMessage();
        var message = new object();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(message);

        messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>())).Returns(ValueTask.FromResult(true));

        await pipeline.ExecuteAsync();

        messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>()), Times.Once);

        Assert.That(pipeline.State.GetMessageHandlerInvoked(), Is.True);

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_execute_missing_handler_async()
    {
        var errorQueue = new Mock<IQueue>();
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();
        var messageNotHandledCount = 0;
        var handlerExceptionCount = 0;

        var observer = new HandleMessageObserver(Options.Create(new ServiceBusOptions()), messageHandlerInvoker.Object, serializer.Object);

        observer.MessageNotHandled += (_, _) =>
        {
            messageNotHandledCount++;
        };

        observer.HandlerException += (_, _) =>
        {
            handlerExceptionCount++;
        };

        errorQueue.Setup(m => m.Uri).Returns(new QueueUri("queue://configuration/name"));

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnHandleMessage>();

        var transportMessage = new TransportMessage();
        var message = new object();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(message);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>())).Returns(ValueTask.FromResult(false));

        await pipeline.ExecuteAsync();

        messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>()));

        errorQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
        serializer.Verify(m => m.SerializeAsync(transportMessage));

        Assert.That(pipeline.State.GetMessageHandlerInvoked(), Is.False);
        Assert.That(messageNotHandledCount, Is.EqualTo(1));
        Assert.That(handlerExceptionCount, Is.Zero);

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_remove_messages_not_handled_async()
    {
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();
        var messageNotHandledCount = 0;
        var handlerExceptionCount = 0;

        var observer = new HandleMessageObserver(Options.Create(new ServiceBusOptions { RemoveMessagesNotHandled = true }), messageHandlerInvoker.Object, serializer.Object);

        observer.MessageNotHandled += (_, _) =>
        {
            messageNotHandledCount++;
        };

        observer.HandlerException += (_, _) =>
        {
            handlerExceptionCount++;
        };

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnHandleMessage>();

        var transportMessage = new TransportMessage();
        var message = new object();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(message);

        messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>())).Returns(ValueTask.FromResult(false));

        await pipeline.ExecuteAsync();

        messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>()), Times.Once);

        Assert.That(pipeline.State.GetMessageHandlerInvoked(), Is.False);
        Assert.That(messageNotHandledCount, Is.EqualTo(1));
        Assert.That(handlerExceptionCount, Is.Zero);

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_fail_on_missing_error_queue_async()
    {
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();
        var messageNotHandledCount = 0;
        var handlerExceptionCount = 0;

        var observer = new HandleMessageObserver(Options.Create(new ServiceBusOptions()), messageHandlerInvoker.Object, serializer.Object);

        observer.MessageNotHandled += (_, _) =>
        {
            messageNotHandledCount++;
        };

        observer.HandlerException += (_, _) =>
        {
            handlerExceptionCount++;
        };

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnHandleMessage>();

        var transportMessage = new TransportMessage();
        var message = new object();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(message);

        messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>())).Returns(ValueTask.FromResult(false));

        Assert.ThrowsAsync<Core.Pipelines.PipelineException>(() => pipeline.ExecuteAsync());

        messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>()), Times.Once);

        Assert.That(pipeline.State.GetMessageHandlerInvoked(), Is.False);
        Assert.That(messageNotHandledCount, Is.EqualTo(1));
        Assert.That(handlerExceptionCount, Is.EqualTo(1));

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();

        await Task.CompletedTask;
    }
}