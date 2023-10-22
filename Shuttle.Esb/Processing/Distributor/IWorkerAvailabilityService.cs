using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IWorkerAvailabilityService
    {
        AvailableWorker GetAvailableWorker();
        void ReturnAvailableWorker(AvailableWorker availableWorker);
        void WorkerAvailable(WorkerThreadAvailableCommand message);
        void WorkerStarted(WorkerStartedEvent message);
        void RemoveByThread(WorkerThreadAvailableCommand message);
        Task<AvailableWorker> GetAvailableWorkerAsync();
        Task ReturnAvailableWorkerAsync(AvailableWorker availableWorker);
        Task WorkerAvailableAsync(WorkerThreadAvailableCommand message);
        Task WorkerStartedAsyncAsync(WorkerStartedEvent message);
        Task RemoveByThread(WorkerThreadAvailableCommand message);
    }
}