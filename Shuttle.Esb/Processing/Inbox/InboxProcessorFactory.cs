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

        public InboxProcessorFactory(ServiceBusOptions serviceBusOptions, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
        {
            _serviceBusOptions = Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _pipelineThreadActivity = Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));
        }

        public IProcessor Create()
        {
            return new InboxProcessor(new ThreadActivity(_serviceBusOptions.Inbox?.DurationToSleepWhenIdle ?? ServiceBusOptions.DefaultDurationToSleepWhenIdle), _pipelineFactory, _pipelineThreadActivity);
        }
    }
}