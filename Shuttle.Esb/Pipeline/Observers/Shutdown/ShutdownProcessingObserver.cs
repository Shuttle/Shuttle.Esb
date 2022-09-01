using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public interface IShutdownProcessingObserver : IPipelineObserver<OnStopping>
    {
    }

    public class ShutdownProcessingObserver : IShutdownProcessingObserver
    {
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IQueueService _queueService;

        public ShutdownProcessingObserver(IServiceBusConfiguration serviceBusConfiguration, IQueueService queueService)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(queueService, nameof(queueService));

            _serviceBusConfiguration = serviceBusConfiguration;
            _queueService = queueService;
        }

        public void Execute(OnStopping pipelineEvent)
        {
            if (_serviceBusConfiguration.HasControlInbox())
            {
                _serviceBusConfiguration.ControlInbox.WorkQueue.AttemptDispose();
                _serviceBusConfiguration.ControlInbox.ErrorQueue.AttemptDispose();
            }

            if (_serviceBusConfiguration.HasInbox())
            {
                _serviceBusConfiguration.Inbox.WorkQueue.AttemptDispose();
                _serviceBusConfiguration.Inbox.DeferredQueue.AttemptDispose();
                _serviceBusConfiguration.Inbox.ErrorQueue.AttemptDispose();
            }

            if (_serviceBusConfiguration.HasOutbox())
            {
                _serviceBusConfiguration.Outbox.WorkQueue.AttemptDispose();
                _serviceBusConfiguration.Outbox.ErrorQueue.AttemptDispose();
            }

            if (_serviceBusConfiguration.IsWorker())
            {
                _serviceBusConfiguration.Worker.DistributorControlInboxWorkQueue.AttemptDispose();
            }

            _queueService.AttemptDispose();
        }
    }
}