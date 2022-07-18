using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class ControlInboxProcessorFactory : IProcessorFactory
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPipelineThreadActivity _pipelineThreadActivity;
        private readonly ServiceBusOptions _serviceBusOptions;

        public ControlInboxProcessorFactory(ServiceBusOptions serviceBusOptions,
            IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));

            _serviceBusOptions = serviceBusOptions;
            _pipelineFactory = pipelineFactory;
            _pipelineThreadActivity = pipelineThreadActivity;
        }

        public IProcessor Create()
        {
            return new ControlInboxProcessor(new ThreadActivity(_serviceBusOptions.ControlInbox.DurationToSleepWhenIdle), _pipelineFactory, _pipelineThreadActivity);
        }
    }
}