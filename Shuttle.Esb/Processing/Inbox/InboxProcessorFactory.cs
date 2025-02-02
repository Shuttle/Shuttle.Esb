using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public interface IInboxProcessorFactory : IProcessorFactory
{
}

public class InboxProcessorFactory : IInboxProcessorFactory
{
    private readonly IPipelineFactory _pipelineFactory;
    private readonly IPipelineThreadActivity _pipelineThreadActivity;
    private readonly ServiceBusOptions _serviceBusOptions;

    public InboxProcessorFactory(ServiceBusOptions serviceBusOptions, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
    {
        _serviceBusOptions = Guard.AgainstNull(serviceBusOptions);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _pipelineThreadActivity = Guard.AgainstNull(pipelineThreadActivity);
    }

    public IProcessor Create()
    {
        return new InboxProcessor(new ThreadActivity(_serviceBusOptions.Inbox!.DurationToSleepWhenIdle), _pipelineFactory, _pipelineThreadActivity);
    }
}