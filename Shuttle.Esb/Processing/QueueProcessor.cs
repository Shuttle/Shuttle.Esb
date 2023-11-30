using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public abstract class QueueProcessor<TPipeline> : IProcessor
        where TPipeline : IPipeline
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPipelineThreadActivity _pipelineThreadActivity;
        private readonly IThreadActivity _threadActivity;

        protected QueueProcessor(IThreadActivity threadActivity, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
        {
            Guard.AgainstNull(threadActivity, nameof(threadActivity));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));

            _threadActivity = threadActivity;
            _pipelineFactory = pipelineFactory;
            _pipelineThreadActivity = pipelineThreadActivity;
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken, bool sync)
        {
            var messagePipeline = _pipelineFactory.GetPipeline<TPipeline>();

            try
            {
                messagePipeline.State.ResetWorking();

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