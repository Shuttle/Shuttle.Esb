namespace Shuttle.Esb
{
	public interface IWorkerAvailabilityManager
	{
		AvailableWorker GetAvailableWorker();
		void ReturnAvailableWorker(AvailableWorker availableWorker);

		void WorkerAvailable(WorkerThreadAvailableCommand message);
		void WorkerStarted(WorkerStartedEvent message);
		void RemoveByThread(WorkerThreadAvailableCommand message);
	}
}