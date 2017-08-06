using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ShutdownProcessingObserver :
        IPipelineObserver<OnStopping>,
        IPipelineObserver<OnDisposeQueues>,
        IPipelineObserver<OnStopped>
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IServiceBusEvents _events;
        private readonly IQueueManager _queueManager;

        public ShutdownProcessingObserver(IServiceBusConfiguration configuration, IServiceBusEvents events,
            IQueueManager queueManager)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(events, nameof(events));

            _configuration = configuration;
            _events = events;
            _queueManager = queueManager;
        }

        public void Execute(OnDisposeQueues pipelineEvent)
        {
            if (_configuration.HasControlInbox)
            {
                _configuration.ControlInbox.WorkQueue.AttemptDispose();
                _configuration.ControlInbox.ErrorQueue.AttemptDispose();
            }

            if (_configuration.HasInbox)
            {
                _configuration.Inbox.WorkQueue.AttemptDispose();
                _configuration.Inbox.DeferredQueue.AttemptDispose();
                _configuration.Inbox.ErrorQueue.AttemptDispose();
            }

            if (_configuration.HasOutbox)
            {
                _configuration.Outbox.WorkQueue.AttemptDispose();
                _configuration.Outbox.ErrorQueue.AttemptDispose();
            }

            if (_configuration.IsWorker)
            {
                _configuration.Worker.DistributorControlInboxWorkQueue.AttemptDispose();
            }

            _queueManager.AttemptDispose();
        }

        public void Execute(OnStopped pipelineEvent)
        {
            _events.OnStopped(this, new PipelineEventEventArgs(pipelineEvent));
        }

        public void Execute(OnStopping pipelineEvent)
        {
            _events.OnStopping(this, new PipelineEventEventArgs(pipelineEvent));
        }
    }
}