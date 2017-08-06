using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class OutboxProcessorFactory : IProcessorFactory
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IServiceBusEvents _events;
        private readonly IPipelineFactory _pipelineFactory;

        public OutboxProcessorFactory(IServiceBusConfiguration configuration, IServiceBusEvents events,
            IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _configuration = configuration;
            _events = events;
            _pipelineFactory = pipelineFactory;
        }

        public IProcessor Create()
        {
            return new OutboxProcessor(_events, new ThreadActivity(_configuration.Outbox), _pipelineFactory);
        }
    }
}