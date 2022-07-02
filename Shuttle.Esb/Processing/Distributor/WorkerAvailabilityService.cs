using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Esb
{
    public class WorkerAvailabilityService : IWorkerAvailabilityService
    {
        private static readonly object Lock = new object();

        private readonly Dictionary<string, List<AvailableWorker>> _workers =
            new Dictionary<string, List<AvailableWorker>>();

        public AvailableWorker GetAvailableWorker()
        {
            lock (Lock)
            {
                KeyValuePair<string, List<AvailableWorker>>? worker = null;

                foreach (var w in _workers)
                {
                    if (worker == null)
                    {
                        worker = w;
                    }
                    else
                    {
                        if (w.Value.Count > worker.Value.Value.Count)
                        {
                            worker = w;
                        }
                    }
                }

                if (worker.HasValue)
                {
                    if (worker.Value.Value.Count == 0)
                    {
                        return null;
                    }

                    var result = worker.Value.Value[0];

                    worker.Value.Value.RemoveAt(0);

                    return result;
                }

                return null;
            }
        }

        public void WorkerAvailable(WorkerThreadAvailableCommand message)
        {
            lock (Lock)
            {
                GetAvailableWorkers(message.InboxWorkQueueUri).Add(new AvailableWorker(message));
            }
        }

        public void ReturnAvailableWorker(AvailableWorker availableWorker)
        {
            if (availableWorker == null)
            {
                return;
            }

            lock (Lock)
            {
                GetAvailableWorkers(availableWorker.InboxWorkQueueUri).Add(availableWorker);
            }
        }

        public void WorkerStarted(WorkerStartedEvent message)
        {
            lock (Lock)
            {
                _workers[message.InboxWorkQueueUri] = GetAvailableWorkers(message.InboxWorkQueueUri)
                    .Where(availableWorker => availableWorker.WorkerSendDate < message.DateStarted)
                    .ToList();
            }
        }

        public void RemoveByThread(WorkerThreadAvailableCommand message)
        {
            lock (Lock)
            {
                GetAvailableWorkers(message.InboxWorkQueueUri)
                    .RemoveAll(candidate => candidate.ManagedThreadId == message.ManagedThreadId);
            }
        }

        private List<AvailableWorker> GetAvailableWorkers(string inboxWorkQueueUri)
        {
            if (!_workers.TryGetValue(inboxWorkQueueUri, out var worker))
            {
                worker = new List<AvailableWorker>();
                _workers.Add(inboxWorkQueueUri, worker);
            }

            return worker;
        }
    }
}