using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class WorkerStartedHandler : IMessageHandler<WorkerStartedEvent>
    {
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        public WorkerStartedHandler(IWorkerAvailabilityService workerAvailabilityService)
        {
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));

            _workerAvailabilityService = workerAvailabilityService;
        }

        public async Task ProcessMessage(IHandlerContext<WorkerStartedEvent> context)
        {
            _workerAvailabilityService.WorkerStarted(context.Message);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}