using System.Diagnostics;
using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class InboxProcessor : IProcessor
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IThreadActivity _threadActivity;
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        public InboxProcessor(IServiceBusConfiguration configuration,
            IThreadActivity threadActivity, IWorkerAvailabilityService workerAvailabilityService,
            IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(threadActivity, nameof(threadActivity));
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _configuration = configuration;
            _threadActivity = threadActivity;
            _workerAvailabilityService = workerAvailabilityService;
            _pipelineFactory = pipelineFactory;
        }

        [DebuggerNonUserCode]
        void IProcessor.Execute(CancellationToken cancellationToken)
        {
            Execute(cancellationToken);
        }

        public virtual void Execute(CancellationToken cancellationToken)
        {
            var availableWorker = _workerAvailabilityService.GetAvailableWorker();

            if (_configuration.Inbox.Distribute && availableWorker == null)
            {
                _threadActivity.Waiting(cancellationToken);

                return;
            }

            var messagePipeline = availableWorker == null
                ? _pipelineFactory.GetPipeline<InboxMessagePipeline>()
                : (IPipeline) _pipelineFactory.GetPipeline<DistributorPipeline>();

            try
            {
                messagePipeline.State.SetAvailableWorker(availableWorker);
                messagePipeline.State.ResetWorking();
                messagePipeline.State.SetCancellationToken(cancellationToken);
                messagePipeline.State.SetTransportMessage(null);
                messagePipeline.State.SetReceivedMessage(null);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                messagePipeline.Execute();

                if (messagePipeline.State.GetWorking())
                {
                    _threadActivity.Working();
                }
                else
                {
                    _threadActivity.Waiting(cancellationToken);
                }
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }
    }
}