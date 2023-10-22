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

    [Test]
    public void Should_be_able_to_execute_successful_invoke()
    {
        Should_be_able_to_execute_successful_invoke_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_execute_successful_invoke_async()
    {
        await Should_be_able_to_execute_successful_invoke_async(false);
    }

    private async Task Should_be_able_to_execute_successful_invoke_async(bool sync)
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

        if (sync)
        {
            messageHandlerInvoker.Setup(m => m.Invoke(It.IsAny<IPipelineEvent>())).Returns(MessageHandlerInvokeResult.InvokedHandler("assembly-qualified-name"));

            pipeline.Execute();

            messageHandlerInvoker.Verify(m => m.Invoke(It.IsAny<IPipelineEvent>()), Times.Once);
        }
        else
        {
            messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineEvent>())).Returns(Task.FromResult(MessageHandlerInvokeResult.InvokedHandler("assembly-qualified-name")));

            await pipeline.ExecuteAsync();

            messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineEvent>()), Times.Once);
        }

        Assert.That(pipeline.State.GetMessageHandlerInvokeResult().Invoked, Is.True);

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_execute_missing_handler()
    {
        Should_be_able_to_execute_missing_handler_async(false).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_execute_missing_handler_async()
    {
        await Should_be_able_to_execute_missing_handler_async(true);
    }

    private async Task Should_be_able_to_execute_missing_handler_async(bool sync)
    {
        var errorQueue = new Mock<IQueue>();
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();
        var messageNotHandledCount = 0;
        var handlerExceptionCount = 0;

        var observer = new HandleMessageObserver(Options.Create(new ServiceBusOptions()), messageHandlerInvoker.Object, serializer.Object);

        observer.MessageNotHandled += (sender, args) =>
        {
            messageNotHandledCount++;
        };

        observer.HandlerException += (sender, args) =>
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

        if (sync)
        {
            messageHandlerInvoker.Setup(m => m.Invoke(It.IsAny<IPipelineEvent>())).Returns(MessageHandlerInvokeResult.MissingHandler());

            pipeline.Execute();

            messageHandlerInvoker.Verify(m => m.Invoke(It.IsAny<IPipelineEvent>()));

            errorQueue.Verify(m => m.Enqueue(transportMessage, It.IsAny<Stream>()), Times.Once);
            serializer.Verify(m => m.Serialize(transportMessage), Times.Once);
        }
        else
        {
            messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineEvent>())).Returns(Task.FromResult(MessageHandlerInvokeResult.MissingHandler()));

            await pipeline.ExecuteAsync();

            messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineEvent>()));

            errorQueue.Verify(m => m.EnqueueAsync(transportMessage, It.IsAny<Stream>()), Times.Once);
            serializer.Verify(m => m.SerializeAsync(transportMessage));
        }

        Assert.That(pipeline.State.GetMessageHandlerInvokeResult().Invoked, Is.False);
        Assert.That(messageNotHandledCount, Is.EqualTo(1));
        Assert.That(handlerExceptionCount, Is.Zero);

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_remove_messages_not_handled()
    {
        Should_be_able_to_remove_messages_not_handled_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_remove_messages_not_handled_async()
    {
        await Should_be_able_to_remove_messages_not_handled_async(false);
    }

    private async Task Should_be_able_to_remove_messages_not_handled_async(bool sync)
    {
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();
        var messageNotHandledCount = 0;
        var handlerExceptionCount = 0;

        var observer = new HandleMessageObserver(Options.Create(new ServiceBusOptions { RemoveMessagesNotHandled = true }), messageHandlerInvoker.Object, serializer.Object);

        observer.MessageNotHandled += (sender, args) =>
        {
            messageNotHandledCount++;
        };

        observer.HandlerException += (sender, args) =>
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

        if (sync)
        {
            messageHandlerInvoker.Setup(m => m.Invoke(It.IsAny<IPipelineEvent>())).Returns(MessageHandlerInvokeResult.MissingHandler());

            pipeline.Execute();

            messageHandlerInvoker.Verify(m => m.Invoke(It.IsAny<IPipelineEvent>()), Times.Once);
        }
        else
        {
            messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineEvent>())).Returns(Task.FromResult(MessageHandlerInvokeResult.MissingHandler()));

            await pipeline.ExecuteAsync();

            messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineEvent>()), Times.Once);
        }

        Assert.That(pipeline.State.GetMessageHandlerInvokeResult().Invoked, Is.False);
        Assert.That(messageNotHandledCount, Is.EqualTo(1));
        Assert.That(handlerExceptionCount, Is.Zero);

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_fail_on_missing_error_queue()
    {
        Should_fail_on_missing_error_queue_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_fail_on_missing_error_queue_async()
    {
        await Should_fail_on_missing_error_queue_async(false);
    }

    private async Task Should_fail_on_missing_error_queue_async(bool sync)
    {
        var messageHandlerInvoker = new Mock<IMessageHandlerInvoker>();
        var serializer = new Mock<ISerializer>();
        var messageNotHandledCount = 0;
        var handlerExceptionCount = 0;

        var observer = new HandleMessageObserver(Options.Create(new ServiceBusOptions()), messageHandlerInvoker.Object, serializer.Object);

        observer.MessageNotHandled += (sender, args) =>
        {
            messageNotHandledCount++;
        };

        observer.HandlerException += (sender, args) =>
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

        if (sync)
        {
            messageHandlerInvoker.Setup(m => m.Invoke(It.IsAny<IPipelineEvent>())).Returns(MessageHandlerInvokeResult.MissingHandler());

            Assert.Throws<Core.Pipelines.PipelineException>(() => pipeline.Execute());

            messageHandlerInvoker.Verify(m => m.Invoke(It.IsAny<IPipelineEvent>()), Times.Once);
        }
        else
        {
            messageHandlerInvoker.Setup(m => m.InvokeAsync(It.IsAny<IPipelineEvent>())).Returns(Task.FromResult(MessageHandlerInvokeResult.MissingHandler()));

            Assert.ThrowsAsync<Core.Pipelines.PipelineException>(() => pipeline.ExecuteAsync());

            messageHandlerInvoker.Verify(m => m.InvokeAsync(It.IsAny<IPipelineEvent>()), Times.Once);
        }

        Assert.That(pipeline.State.GetMessageHandlerInvokeResult().Invoked, Is.False);
        Assert.That(messageNotHandledCount, Is.EqualTo(1));
        Assert.That(handlerExceptionCount, Is.EqualTo(1));

        messageHandlerInvoker.VerifyNoOtherCalls();
        serializer.VerifyNoOtherCalls();

        await Task.CompletedTask;
    }
}