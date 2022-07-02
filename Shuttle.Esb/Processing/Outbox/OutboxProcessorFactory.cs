using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class OutboxProcessorFactory : IProcessorFactory
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IPipelineFactory _pipelineFactory;

        public OutboxProcessorFactory(IServiceBusConfiguration configuration,
            IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _configuration = configuration;
            _pipelineFactory = pipelineFactory;
        }

        public IProcessor Create()
        {
            return new OutboxProcessor(new ThreadActivity(_configuration.Outbox), _pipelineFactory);
        }
    }
}