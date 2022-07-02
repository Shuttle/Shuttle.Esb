using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class ControlInboxProcessorFactory : IProcessorFactory
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IPipelineFactory _pipelineFactory;

        public ControlInboxProcessorFactory(IServiceBusConfiguration configuration,
            IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _configuration = configuration;
            _pipelineFactory = pipelineFactory;
        }

        public IProcessor Create()
        {
            return new ControlInboxProcessor(new ThreadActivity(_configuration.ControlInbox),
                _pipelineFactory);
        }
    }
}