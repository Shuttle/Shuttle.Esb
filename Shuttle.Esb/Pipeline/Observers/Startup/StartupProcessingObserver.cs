using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public interface IStartupProcessingObserver :
    IPipelineObserver<OnCreatePhysicalQueues>,
    IPipelineObserver<OnConfigureThreadPools>,
    IPipelineObserver<OnStartThreadPools>
{
}

public class StartupProcessingObserver : IStartupProcessingObserver
{
    private readonly IDeferredMessageProcessor _deferredMessageProcessor;
    private readonly IPipelineFactory _pipelineFactory;
    private readonly IPipelineThreadActivity _pipelineThreadActivity;
    private readonly IProcessorThreadPoolFactory _processorThreadPoolFactory;
    private readonly IServiceBusConfiguration _serviceBusConfiguration;
    private readonly ServiceBusOptions _serviceBusOptions;

    public StartupProcessingObserver(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IDeferredMessageProcessor deferredMessageProcessor, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity, IProcessorThreadPoolFactory processorThreadPoolFactory)
    {
        _serviceBusOptions = Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value);
        _deferredMessageProcessor = Guard.AgainstNull(deferredMessageProcessor);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _pipelineThreadActivity = Guard.AgainstNull(pipelineThreadActivity);
        _serviceBusConfiguration = Guard.AgainstNull(serviceBusConfiguration);
        _processorThreadPoolFactory = Guard.AgainstNull(processorThreadPoolFactory);
    }

    public async Task ExecuteAsync(IPipelineContext<OnCreatePhysicalQueues> pipelineContext)
    {
        if (!_serviceBusOptions.CreatePhysicalQueues)
        {
            return;
        }

        Guard.Against<InvalidOperationException>(_serviceBusConfiguration.HasInbox() && _serviceBusConfiguration.Inbox!.WorkQueue == null && string.IsNullOrEmpty(_serviceBusOptions.Inbox!.WorkQueueUri), string.Format(Resources.RequiredQueueUriMissingException, "Inbox.WorkQueueUri"));
        Guard.Against<InvalidOperationException>(_serviceBusConfiguration.HasOutbox() && _serviceBusConfiguration.Outbox!.WorkQueue == null && string.IsNullOrEmpty(_serviceBusOptions.Outbox!.WorkQueueUri), string.Format(Resources.RequiredQueueUriMissingException, "Outbox.WorkQueueUri"));

        await _serviceBusConfiguration.CreatePhysicalQueuesAsync().ConfigureAwait(false);
    }

    public async Task ExecuteAsync(IPipelineContext<OnConfigureThreadPools> pipelineContext)
    {
        if (_serviceBusConfiguration.HasInbox() && _serviceBusConfiguration.Inbox!.HasDeferredQueue())
        {
            pipelineContext.Pipeline.State.Add("DeferredMessageThreadPool", _processorThreadPoolFactory.Create(
                "DeferredMessageProcessor",
                1,
                new DeferredMessageProcessorFactory(_deferredMessageProcessor),
                _serviceBusOptions.ProcessorThread));
        }

        if (_serviceBusConfiguration.HasInbox())
        {
            pipelineContext.Pipeline.State.Add("InboxThreadPool", _processorThreadPoolFactory.Create(
                "InboxProcessor",
                _serviceBusOptions.Inbox!.ThreadCount,
                new InboxProcessorFactory(_serviceBusOptions, _pipelineFactory, _pipelineThreadActivity), _serviceBusOptions.ProcessorThread));
        }

        if (_serviceBusConfiguration.HasOutbox())
        {
            pipelineContext.Pipeline.State.Add("OutboxThreadPool", _processorThreadPoolFactory.Create(
                "OutboxProcessor",
                _serviceBusOptions.Outbox!.ThreadCount,
                new OutboxProcessorFactory(_serviceBusOptions, _pipelineFactory, _pipelineThreadActivity),
                _serviceBusOptions.ProcessorThread));
        }

        await Task.CompletedTask;
    }

    public async Task ExecuteAsync(IPipelineContext<OnStartThreadPools> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext.Pipeline.State);

        var inboxThreadPool = state.Get<IProcessorThreadPool>("InboxThreadPool");
        var controlInboxThreadPool = state.Get<IProcessorThreadPool>("ControlInboxThreadPool");
        var outboxThreadPool = state.Get<IProcessorThreadPool>("OutboxThreadPool");
        var deferredMessageThreadPool = state.Get<IProcessorThreadPool>("DeferredMessageThreadPool");

        if (inboxThreadPool != null)
        {
            await inboxThreadPool.StartAsync();
        }

        if (controlInboxThreadPool != null)
        {
            await controlInboxThreadPool.StartAsync();
        }

        if (outboxThreadPool != null)
        {
            await outboxThreadPool.StartAsync();
        }

        if (deferredMessageThreadPool != null)
        {
            await deferredMessageThreadPool.StartAsync();
        }
    }
}