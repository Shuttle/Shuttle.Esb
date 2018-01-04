using System.Diagnostics;
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
        void IProcessor.Execute(IThreadState state)
        {
            Execute(state);
        }

        public virtual void Execute(IThreadState state)
        {
            var messagePipeline = _pipelineFactory.GetPipeline<TPipeline>();

            try
            {
                messagePipeline.State.ResetWorking();
                messagePipeline.State.Replace(StateKeys.ActiveState, state);

                messagePipeline.Execute();

                if (messagePipeline.State.GetWorking())
                {
                    _events.OnThreadWorking(this, new ThreadStateEventArgs(typeof(TPipeline)));

                    _threadActivity.Working();
                }
                else
                {
                    _events.OnThreadWaiting(this, new ThreadStateEventArgs(typeof(TPipeline)));

                    _threadActivity.Waiting(state);
                }
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(messagePipeline);
            }
        }
    }
}