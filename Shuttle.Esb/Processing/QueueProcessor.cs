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
        private readonly IServiceBusEvents _events;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IThreadActivity _threadActivity;

        protected QueueProcessor(IServiceBusEvents events, IThreadActivity threadActivity,
            IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(threadActivity, nameof(threadActivity));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _events = events;
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
            Guard.AgainstNull(cancellationToken, nameof(cancellationToken));

            var messagePipeline = _pipelineFactory.GetPipeline<TPipeline>();

            try
            {
                messagePipeline.State.ResetWorking();
                messagePipeline.State.Replace(StateKeys.CancellationToken, cancellationToken);

                messagePipeline.Execute();

                if (messagePipeline.State.GetWorking())
                {
                    _events.OnThreadWorking(this, new ThreadStateEventArgs(typeof(TPipeline)));

                    _threadActivity.Working();
                }
                else
                {
                    _events.OnThreadWaiting(this, new ThreadStateEventArgs(typeof(TPipeline)));

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