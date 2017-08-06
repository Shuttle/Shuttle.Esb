using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ServiceBusEvents : IServiceBusEvents
    {
        public event EventHandler<PipelineExceptionEventArgs> BeforePipelineExceptionHandled = delegate { };

        public void OnBeforePipelineExceptionHandled(object sender, PipelineExceptionEventArgs args)
        {
            BeforePipelineExceptionHandled.Invoke(sender, args);
        }

        public void OnAfterPipelineExceptionHandled(object sender, PipelineExceptionEventArgs args)
        {
            AfterPipelineExceptionHandled.Invoke(sender, args);
        }

        public void OnTransportMessageDeserializationException(object sender, DeserializationExceptionEventArgs args)
        {
            TransportMessageDeserializationException.Invoke(sender, args);
        }

        public void OnMessageDeserializationException(object sender, DeserializationExceptionEventArgs args)
        {
            TransportMessageDeserializationException.Invoke(sender, args);
        }

        public void OnQueueEmpty(object sender, QueueEmptyEventArgs args)
        {
            QueueEmpty.Invoke(sender, args);
        }

        public void OnMessageNotHandled(object sender, MessageNotHandledEventArgs args)
        {
            MessageNotHandled.Invoke(sender, args);
        }

        public void OnHandlerException(object sender, HandlerExceptionEventArgs args)
        {
            HandlerException.Invoke(sender, args);
        }

        public void OnThreadWorking(object sender, ThreadStateEventArgs args)
        {
            ThreadWorking.Invoke(sender, args);
        }

        public void OnThreadWaiting(object sender, ThreadStateEventArgs args)
        {
            ThreadWaiting(sender, args);
        }

        public void OnStarting(object sender, PipelineEventEventArgs args)
        {
            Starting(sender, args);
        }

        public void OnStarted(object sender, PipelineEventEventArgs args)
        {
            Started(sender, args);
        }

        public void OnStopping(object sender, PipelineEventEventArgs args)
        {
            Stopping(sender, args);
        }

        public void OnStopped(object sender, PipelineEventEventArgs args)
        {
            Stopped(sender, args);
        }

        public event EventHandler Starting = delegate { };
        public event EventHandler Started = delegate { };
        public event EventHandler Stopping = delegate { };
        public event EventHandler Stopped = delegate { };
        public event EventHandler<PipelineExceptionEventArgs> AfterPipelineExceptionHandled = delegate { };

        public event EventHandler<DeserializationExceptionEventArgs> TransportMessageDeserializationException =
            delegate { };

        public event EventHandler<DeserializationExceptionEventArgs> MessageDeserializationException = delegate { };
        public event EventHandler<QueueEmptyEventArgs> QueueEmpty = delegate { };
        public event EventHandler<MessageNotHandledEventArgs> MessageNotHandled = delegate { };
        public event EventHandler<HandlerExceptionEventArgs> HandlerException = delegate { };
        public event EventHandler<ThreadStateEventArgs> ThreadWorking = delegate { };
        public event EventHandler<ThreadStateEventArgs> ThreadWaiting = delegate { };
    }
}