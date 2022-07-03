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
        private readonly IThreadActivity _threadActivity;

        protected QueueProcessor(IThreadActivity threadActivity, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(threadActivity, nameof(threadActivity));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _threadActivity = threadActivity;
            _pipelineFactory = pipelineFactory;
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