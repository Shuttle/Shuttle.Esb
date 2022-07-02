using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IDistributorMessageObserver : 
        IPipelineObserver<OnHandleDistributeMessage>, 
        IPipelineObserver<OnAbortPipeline>
    {
    }

    public class DistributorMessageObserver : IDistributorMessageObserver
    {
        private readonly IWorkerAvailabilityManager _workerAvailabilityManager;

        public DistributorMessageObserver(IWorkerAvailabilityManager workerAvailabilityManager)
        {
            Guard.AgainstNull(workerAvailabilityManager, nameof(workerAvailabilityManager));

            _workerAvailabilityManager = workerAvailabilityManager;
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            _workerAvailabilityManager.ReturnAvailableWorker(state.GetAvailableWorker());
        }

        public void Execute(OnHandleDistributeMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            transportMessage.RecipientInboxWorkQueueUri = state.GetAvailableWorker().InboxWorkQueueUri;

            state.SetTransportMessage(transportMessage);
            state.SetTransportMessageReceived(null);
        }
    }
}