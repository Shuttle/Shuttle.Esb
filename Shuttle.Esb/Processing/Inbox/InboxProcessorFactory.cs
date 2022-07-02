using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class InboxProcessorFactory : IProcessorFactory
    {
        private readonly IServiceBus _bus;
        private readonly IServiceBusConfiguration _configuration;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        public InboxProcessorFactory(IServiceBus bus, IServiceBusConfiguration configuration,
            IWorkerAvailabilityService workerAvailabilityService, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(bus, nameof(bus));
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _bus = bus;
            _configuration = configuration;
            _workerAvailabilityService = workerAvailabilityService;
            _pipelineFactory = pipelineFactory;
        }

        public IProcessor Create()
        {
            var threadActivity = new ThreadActivity(_configuration.Inbox);

            return new InboxProcessor(_configuration, _configuration.IsWorker
                ? (IThreadActivity) new WorkerThreadActivity(_bus, _configuration, threadActivity)
                : threadActivity, _workerAvailabilityService, _pipelineFactory);
        }
    }
}