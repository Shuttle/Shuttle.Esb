using System.Threading.Tasks;
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

        public async Task Execute(OnStopping pipelineEvent)
        {
            if (_serviceBusConfiguration.HasControlInbox())
            {
                _serviceBusConfiguration.ControlInbox.WorkQueue.TryDispose();
                _serviceBusConfiguration.ControlInbox.ErrorQueue.TryDispose();
            }

            if (_serviceBusConfiguration.HasInbox())
            {
                _serviceBusConfiguration.Inbox.WorkQueue.TryDispose();
                _serviceBusConfiguration.Inbox.DeferredQueue.TryDispose();
                _serviceBusConfiguration.Inbox.ErrorQueue.TryDispose();
            }

            if (_serviceBusConfiguration.HasOutbox())
            {
                _serviceBusConfiguration.Outbox.WorkQueue.TryDispose();
                _serviceBusConfiguration.Outbox.ErrorQueue.TryDispose();
            }

            if (_serviceBusConfiguration.IsWorker())
            {
                _serviceBusConfiguration.Worker.DistributorControlInboxWorkQueue.TryDispose();
            }

            _queueService.TryDispose();

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}