using System.Diagnostics;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class InboxProcessor : IProcessor
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IServiceBusEvents _events;
        protected readonly IThreadActivity _threadActivity;
        private readonly IWorkerAvailabilityManager _workerAvailabilityManager;
        private readonly IPipelineFactory _pipelineFactory;

        public InboxProcessor(IServiceBusConfiguration configuration, IServiceBusEvents events, IThreadActivity threadActivity, IWorkerAvailabilityManager workerAvailabilityManager, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(events, "events");
            Guard.AgainstNull(threadActivity, "threadActivity");
            Guard.AgainstNull(workerAvailabilityManager, "workerAvailabilityManager");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");

            _configuration = configuration;
            _events = events;
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

            if (_configuration.Inbox.Distribute && availableWorker == null)
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
                messagePipeline.State.SetTransportMessage(null);
                messagePipeline.State.SetReceivedMessage(null);

                if (!state.Active)
                {
                    return;
                }

                messagePipeline.Execute();

                if (messagePipeline.State.GetWorking())
                {
                    _events.OnThreadWorking(this, new ThreadStateEventArgs(typeof(InboxMessagePipeline)));

                    _threadActivity.Working();
                }
                else
                {
                    _events.OnThreadWaiting(this, new ThreadStateEventArgs(typeof(InboxMessagePipeline)));

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