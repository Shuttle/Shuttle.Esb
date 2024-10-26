﻿using System;
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

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnHandleMessage>();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Ignore);

        await pipeline.ExecuteAsync();

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();

        pipeline.State.SetProcessingStatus(ProcessingStatus.MessageHandled);

        await pipeline.ExecuteAsync();

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
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

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnHandleMessage>();

        var transportMessage = new TransportMessage();
        var message = new object();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(message);

        messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>())).Returns(Task.FromResult(MessageHandlerInvokeResult.InvokedHandler("assembly-qualified-name")));

        await pipeline.ExecuteAsync();

        messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>()), Times.Once);

        Assert.That(pipeline.State.GetMessageHandlerInvokeResult()!.Invoked, Is.True);

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

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnHandleMessage>();

        var transportMessage = new TransportMessage();
        var message = new object();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(message);
        pipeline.State.SetErrorQueue(errorQueue.Object);

        messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>())).Returns(Task.FromResult(MessageHandlerInvokeResult.MissingHandler()));

        await pipeline.ExecuteAsync();

        messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>()));

        errorQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
        serializer.Verify(m => m.SerializeAsync(transportMessage));

        Assert.That(pipeline.State.GetMessageHandlerInvokeResult()!.Invoked, Is.False);
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

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnHandleMessage>();

        var transportMessage = new TransportMessage();
        var message = new object();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(message);

        messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>())).Returns(Task.FromResult(MessageHandlerInvokeResult.MissingHandler()));

        await pipeline.ExecuteAsync();

        messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>()), Times.Once);

        Assert.That(pipeline.State.GetMessageHandlerInvokeResult()!.Invoked, Is.False);
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

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnHandleMessage>();

        var transportMessage = new TransportMessage();
        var message = new object();

        pipeline.State.SetProcessingStatus(ProcessingStatus.Active);
        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(message);

        messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>())).Returns(Task.FromResult(MessageHandlerInvokeResult.MissingHandler()));

        Assert.ThrowsAsync<Core.Pipelines.PipelineException>(() => pipeline.ExecuteAsync());

        messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineContext<OnHandleMessage>>()), Times.Once);

        Assert.That(pipeline.State.GetMessageHandlerInvokeResult()!.Invoked, Is.False);
        Assert.That(messageNotHandledCount, Is.EqualTo(1));
        Assert.That(handlerExceptionCount, Is.EqualTo(1));

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();

        await Task.CompletedTask;
    }
}