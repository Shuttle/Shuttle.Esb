using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IInboxProcessorFactory : IProcessorFactory
    {
    }

    public class InboxProcessorFactory : IInboxProcessorFactory
    {
        private readonly ServiceBusOptions _serviceBusOptions;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPipelineThreadActivity _pipelineThreadActivity;
        private readonly IServiceBus _serviceBus;
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        public InboxProcessorFactory(ServiceBusOptions serviceBusOptions,
            IServiceBusConfiguration serviceBusConfiguration, IServiceBus serviceBus,
            IWorkerAvailabilityService workerAvailabilityService, IPipelineFactory pipelineFactory,
            IPipelineThreadActivity pipelineThreadActivity)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(serviceBus, nameof(serviceBus));
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));

            _serviceBusOptions = serviceBusOptions;
            _serviceBusConfiguration = serviceBusConfiguration;
            _serviceBus = serviceBus;
            _workerAvailabilityService = workerAvailabilityService;
            _pipelineFactory = pipelineFactory;
            _pipelineThreadActivity = pipelineThreadActivity;
        }

        public IProcessor Create()
        {
            var threadActivity = new ThreadActivity(_serviceBusOptions.Inbox?.DurationToSleepWhenIdle ??
                                                    ServiceBusOptions.DefaultDurationToSleepWhenIdle);
            var inboxProcessorThreadActivity = _serviceBusConfiguration.IsWorker()
                ? (IThreadActivity)new WorkerThreadActivity(_serviceBusOptions, _serviceBus, _serviceBusConfiguration, threadActivity)
                : threadActivity;

            return new InboxProcessor(_serviceBusOptions, inboxProcessorThreadActivity, _workerAvailabilityService,
                _pipelineFactory, _pipelineThreadActivity);
        }
    }
}