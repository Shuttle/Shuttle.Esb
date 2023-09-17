using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class InboxProcessor : IProcessor
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPipelineThreadActivity _pipelineThreadActivity;
        private readonly IThreadActivity _threadActivity;
        private readonly IWorkerAvailabilityService _workerAvailabilityService;
        private readonly ServiceBusOptions _serviceBusOptions;

        public InboxProcessor(ServiceBusOptions serviceBusOptions,
            IThreadActivity threadActivity, IWorkerAvailabilityService workerAvailabilityService,
            IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(threadActivity, nameof(threadActivity));
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));

            _serviceBusOptions = serviceBusOptions;
            _threadActivity = threadActivity;
            _workerAvailabilityService = workerAvailabilityService;
            _pipelineFactory = pipelineFactory;
            _pipelineThreadActivity = pipelineThreadActivity;
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken, bool sync)
        {
            var availableWorker = _workerAvailabilityService.GetAvailableWorker();

            if (_serviceBusOptions.Inbox.Distribute && availableWorker == null)
            {
                if (sync)
                {
                    _threadActivity.Waiting(cancellationToken);
                }
                else
                {
                    await _threadActivity.WaitingAsync(cancellationToken).ConfigureAwait(false);
                }

                _pipelineThreadActivity.OnThreadWaiting(this, new ThreadStateEventArgs(null));

                return;
            }

            var messagePipeline = availableWorker == null
                ? _pipelineFactory.GetPipeline<InboxMessagePipeline>()
                : (IPipeline) _pipelineFactory.GetPipeline<DistributorPipeline>();

            try
            {
                messagePipeline.State.SetAvailableWorker(availableWorker);
                messagePipeline.State.ResetWorking();
                messagePipeline.State.SetTransportMessage(null);
                messagePipeline.State.SetReceivedMessage(null);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (sync)
                {
                    messagePipeline.Execute(cancellationToken);
                }
                else
                {
                    await messagePipeline.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                }

                if (messagePipeline.State.GetWorking())
                {
                    _threadActivity.Working();

                    _pipelineThreadActivity.OnThreadWorking(this, new ThreadStateEventArgs(messagePipeline));
                }
                else
                {
                    _pipelineThreadActivity.OnThreadWaiting(this, new ThreadStateEventArgs(messagePipeline));

                    if (sync)
                    {
                        _threadActivity.Waiting(cancellationToken);
                    }
                    else
                    {
                        await _threadActivity.WaitingAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }

        public void Execute(CancellationToken cancellationToken)
        {
            ExecuteAsync(cancellationToken, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await ExecuteAsync(cancellationToken, false).ConfigureAwait(false);
        }
    }
}