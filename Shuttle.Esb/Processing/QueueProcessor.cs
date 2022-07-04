using System.Diagnostics;
using System.Threading;
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

        [DebuggerNonUserCode]
        void IProcessor.Execute(CancellationToken cancellationToken)
        {
            Execute(cancellationToken);
        }

        public virtual void Execute(CancellationToken cancellationToken)
        {
            var messagePipeline = _pipelineFactory.GetPipeline<TPipeline>();

            try
            {
                messagePipeline.State.ResetWorking();
                messagePipeline.State.Replace(StateKeys.CancellationToken, cancellationToken);

                messagePipeline.Execute();

                if (messagePipeline.State.GetWorking())
                {
                    _threadActivity.Working();

                    _pipelineThreadActivity.OnThreadWorking(this, new ThreadStateEventArgs(messagePipeline));
                }
                else
                {
                    _threadActivity.Waiting(cancellationToken);

                    _pipelineThreadActivity.OnThreadWaiting(this, new ThreadStateEventArgs(messagePipeline));
                }
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }
    }
}