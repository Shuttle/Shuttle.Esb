using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class WorkerStartedHandler : IAsyncMessageHandler<WorkerStartedEvent>
    {
        private readonly IWorkerAvailabilityService _workerAvailabilityService;

        public WorkerStartedHandler(IWorkerAvailabilityService workerAvailabilityService)
        {
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));

            _workerAvailabilityService = workerAvailabilityService;
        }

        public async Task ProcessMessageAsync(IHandlerContext<WorkerStartedEvent> context)
        {
            _workerAvailabilityService.WorkerStarted(context.Message);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}