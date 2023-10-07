using System.Threading.Tasks;
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

        public void Execute(OnHandleDistributeMessage pipelineEvent)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);

            transportMessage.RecipientInboxWorkQueueUri = Guard.AgainstNull(state.GetAvailableWorker(), StateKeys.AvailableWorker).InboxWorkQueueUri;

            state.SetTransportMessage(transportMessage);
            state.SetTransportMessageReceived(null);
        }

        public async Task ExecuteAsync(OnHandleDistributeMessage pipelineEvent)
        {
            Execute(pipelineEvent);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            _workerAvailabilityService.ReturnAvailableWorker(state.GetAvailableWorker());
        }

        public async Task ExecuteAsync(OnAbortPipeline pipelineEvent)
        {
            Execute(pipelineEvent);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}