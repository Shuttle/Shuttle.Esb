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
        private readonly IServiceBusConfiguration _configuration;
        private readonly IQueueService _queueService;

        public ShutdownProcessingObserver(IServiceBusConfiguration configuration, IQueueService queueService)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(queueService, nameof(queueService));

            _configuration = configuration;
            _queueService = queueService;
        }

        public void Execute(OnStopping pipelineEvent)
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

            _queueService.AttemptDispose();
        }
    }
}