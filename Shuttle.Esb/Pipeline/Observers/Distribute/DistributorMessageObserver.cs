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
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        public DistributorMessageObserver(IWorkerAvailabilityService workerAvailabilityService)
        {
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));

            _workerAvailabilityService = workerAvailabilityService;
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            _workerAvailabilityService.ReturnAvailableWorker(state.GetAvailableWorker());
        }

        public void Execute(OnHandleDistributeMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            transportMessage.RecipientUri = state.GetAvailableWorker().Uri;

            state.SetTransportMessage(transportMessage);
            state.SetTransportMessageReceived(null);
        }
    }
}