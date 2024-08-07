using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IStartupProcessingObserver : 
        IPipelineObserver<OnCreatePhysicalQueues>, 
        IPipelineObserver<OnConfigureThreadPools>, 
        IPipelineObserver<OnStartThreadPools>
    {
    }

    public class StartupProcessingObserver : IStartupProcessingObserver
    {
        private readonly IProcessorThreadPoolFactory _processorThreadPoolFactory;
        private readonly ServiceBusOptions _serviceBusOptions;
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPipelineThreadActivity _pipelineThreadActivity;
        private readonly IDeferredMessageProcessor _deferredMessageProcessor;

        public StartupProcessingObserver(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IDeferredMessageProcessor deferredMessageProcessor, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity, IProcessorThreadPoolFactory processorThreadPoolFactory)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));

            _serviceBusOptions = Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            _deferredMessageProcessor = Guard.AgainstNull(deferredMessageProcessor, nameof(deferredMessageProcessor));
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _pipelineThreadActivity = Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));
            _serviceBusConfiguration = Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            _processorThreadPoolFactory = Guard.AgainstNull(processorThreadPoolFactory, nameof(processorThreadPoolFactory));
        }

        public void Execute(OnCreatePhysicalQueues pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnCreatePhysicalQueues pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnCreatePhysicalQueues pipelineEvent, bool sync)
        {
            if (!_serviceBusOptions.CreatePhysicalQueues)
            {
                return;
            }

            if (sync)
            {
                _serviceBusConfiguration.CreatePhysicalQueues();
            }
            else
            {
                await _serviceBusConfiguration.CreatePhysicalQueuesAsync().ConfigureAwait(false);
            }
        }

        public void Execute(OnConfigureThreadPools pipelineEvent)
        {
            if (_serviceBusConfiguration.HasInbox() && _serviceBusConfiguration.Inbox.HasDeferredQueue())
            {
                pipelineEvent.Pipeline.State.Add("DeferredMessageThreadPool", _processorThreadPoolFactory.Create(
                    "DeferredMessageProcessor",
                    1,
                    new DeferredMessageProcessorFactory(_deferredMessageProcessor),
                    _serviceBusOptions.ProcessorThread));
            }

            if (_serviceBusConfiguration.HasInbox())
            {
                pipelineEvent.Pipeline.State.Add("InboxThreadPool", _processorThreadPoolFactory.Create(
                    "InboxProcessor",
                    _serviceBusOptions.Inbox.ThreadCount,
                    new InboxProcessorFactory(_serviceBusOptions, _pipelineFactory, _pipelineThreadActivity),                    _serviceBusOptions.ProcessorThread));
            }

            if (_serviceBusConfiguration.HasOutbox())
            {
                pipelineEvent.Pipeline.State.Add("OutboxThreadPool", _processorThreadPoolFactory.Create(
                    "OutboxProcessor",
                    _serviceBusOptions.Outbox.ThreadCount,
                    new OutboxProcessorFactory(_serviceBusOptions, _pipelineFactory, _pipelineThreadActivity),
                    _serviceBusOptions.ProcessorThread));
            }
        }

        public async Task ExecuteAsync(OnConfigureThreadPools pipelineEvent)
        {
            Execute(pipelineEvent);

            await Task.CompletedTask;
        }

        public void Execute(OnStartThreadPools pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStartThreadPools pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnStartThreadPools pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent?.Pipeline?.State, nameof(pipelineEvent.Pipeline.State));

            var inboxThreadPool = state.Get<IProcessorThreadPool>("InboxThreadPool");
            var controlInboxThreadPool = state.Get<IProcessorThreadPool>("ControlInboxThreadPool");
            var outboxThreadPool = state.Get<IProcessorThreadPool>("OutboxThreadPool");
            var deferredMessageThreadPool = state.Get<IProcessorThreadPool>("DeferredMessageThreadPool");

            if (sync)
            {
                inboxThreadPool?.Start();
                controlInboxThreadPool?.Start();
                outboxThreadPool?.Start();
                deferredMessageThreadPool?.Start();
            }
            else
            {
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
    }
}