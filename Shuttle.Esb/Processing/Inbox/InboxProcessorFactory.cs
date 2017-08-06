using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class InboxProcessorFactory : IProcessorFactory
    {
        private readonly IServiceBus _bus;
        private readonly IServiceBusConfiguration _configuration;
        private readonly IServiceBusEvents _events;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IWorkerAvailabilityManager _workerAvailabilityManager;

        public InboxProcessorFactory(IServiceBus bus, IServiceBusConfiguration configuration, IServiceBusEvents events,
            IWorkerAvailabilityManager workerAvailabilityManager, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(bus, nameof(bus));
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(workerAvailabilityManager, nameof(workerAvailabilityManager));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _bus = bus;
            _configuration = configuration;
            _events = events;
            _workerAvailabilityManager = workerAvailabilityManager;
            _pipelineFactory = pipelineFactory;
        }

        public IProcessor Create()
        {
            var threadActivity = new ThreadActivity(_configuration.Inbox);

            return new InboxProcessor(_configuration, _events, _configuration.IsWorker
                ? (IThreadActivity) new WorkerThreadActivity(_bus, _configuration, threadActivity)
                : threadActivity, _workerAvailabilityManager, _pipelineFactory);
        }
    }
}