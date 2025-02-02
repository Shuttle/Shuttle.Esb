using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public class OutboxProcessorFactory : IProcessorFactory
{
    private readonly IPipelineFactory _pipelineFactory;
    private readonly IPipelineThreadActivity _pipelineThreadActivity;
    private readonly ServiceBusOptions _serviceBusOptions;

    public OutboxProcessorFactory(ServiceBusOptions serviceBusOptions, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
    {
        _serviceBusOptions = Guard.AgainstNull(serviceBusOptions);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _pipelineThreadActivity = Guard.AgainstNull(pipelineThreadActivity);
    }

    public IProcessor Create()
    {
        return new OutboxProcessor(new ThreadActivity(_serviceBusOptions.Outbox!.DurationToSleepWhenIdle), _pipelineFactory, _pipelineThreadActivity);
    }
}