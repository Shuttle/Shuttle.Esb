using System.Threading;
using System.Threading.Tasks;
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

        public InboxProcessor(IThreadActivity threadActivity, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
        {
            _threadActivity = Guard.AgainstNull(threadActivity, nameof(threadActivity));
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _pipelineThreadActivity = Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken, bool sync)
        {
            var messagePipeline = _pipelineFactory.GetPipeline<InboxMessagePipeline>();

            try
            {
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