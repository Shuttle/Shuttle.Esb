using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IServiceBusEvents
    {
        event EventHandler<PipelineExceptionEventArgs> BeforePipelineExceptionHandled;
        event EventHandler<PipelineExceptionEventArgs> AfterPipelineExceptionHandled;
        event EventHandler<DeserializationExceptionEventArgs> TransportMessageDeserializationException;
        event EventHandler<DeserializationExceptionEventArgs> MessageDeserializationException;

        event EventHandler<QueueEmptyEventArgs> QueueEmpty;
        event EventHandler<MessageNotHandledEventArgs> MessageNotHandled;
        event EventHandler<HandlerExceptionEventArgs> HandlerException;

        event EventHandler<ThreadStateEventArgs> ThreadWorking;
        event EventHandler<ThreadStateEventArgs> ThreadWaiting;

        event EventHandler Started;
        event EventHandler Starting;

        event EventHandler Stopped;
        event EventHandler Stopping;

        void OnBeforePipelineExceptionHandled(object sender, PipelineExceptionEventArgs args);
        void OnAfterPipelineExceptionHandled(object sender, PipelineExceptionEventArgs args);
        void OnTransportMessageDeserializationException(object sender, DeserializationExceptionEventArgs args);
        void OnMessageDeserializationException(object sender, DeserializationExceptionEventArgs args);
        void OnQueueEmpty(object sender, QueueEmptyEventArgs args);
        void OnMessageNotHandled(object sender, MessageNotHandledEventArgs args);
        void OnHandlerException(object sender, HandlerExceptionEventArgs args);

        void OnThreadWorking(object sender, ThreadStateEventArgs args);
        void OnThreadWaiting(object sender, ThreadStateEventArgs args);

        void OnStarted(object sender, PipelineEventEventArgs args);
        void OnStarting(object sender, PipelineEventEventArgs args);

        void OnStopped(object sender, PipelineEventEventArgs args);
        void OnStopping(object sender, PipelineEventEventArgs args);
    }
}