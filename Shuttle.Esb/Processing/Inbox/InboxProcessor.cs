using System.Diagnostics;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class InboxProcessor : IProcessor
    {
        protected readonly IServiceBus _bus;
        protected readonly IThreadActivity _threadActivity;
        private readonly IWorkerAvailabilityManager _workerAvailabilityManager;
        private readonly IPipelineFactory _pipelineFactory;

        public InboxProcessor(IServiceBus bus, IThreadActivity threadActivity, IWorkerAvailabilityManager workerAvailabilityManager, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(bus, "bus");
            Guard.AgainstNull(threadActivity, "threadActivity");
            Guard.AgainstNull(workerAvailabilityManager, "workerAvailabilityManager");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");

            _bus = bus;
            _threadActivity = threadActivity;
            _workerAvailabilityManager = workerAvailabilityManager;
            _pipelineFactory = pipelineFactory;
        }

        [DebuggerNonUserCode]
        void IProcessor.Execute(IThreadState state)
        {
            Execute(state);
        }

        public virtual void Execute(IThreadState state)
        {
            var availableWorker = _workerAvailabilityManager.GetAvailableWorker();

            if (_bus.Configuration.Inbox.Distribute && availableWorker == null)
            {
                _threadActivity.Waiting(state);

                return;
            }

            var messagePipeline = availableWorker == null
                ? _pipelineFactory.GetPipeline<InboxMessagePipeline>()
                : (IPipeline)_pipelineFactory.GetPipeline<DistributorPipeline>();

            try
            {
                messagePipeline.State.SetAvailableWorker(availableWorker);
                messagePipeline.State.ResetWorking();
                messagePipeline.State.SetActiveState(state);

                if (!state.Active)
                {
                    return;
                }

                messagePipeline.Execute();

                if (messagePipeline.State.GetWorking())
                {
                    _bus.Events.OnThreadWorking(this, new ThreadStateEventArgs(typeof(InboxMessagePipeline)));

                    _threadActivity.Working();
                }
                else
                {
                    _bus.Events.OnThreadWaiting(this, new ThreadStateEventArgs(typeof(InboxMessagePipeline)));

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