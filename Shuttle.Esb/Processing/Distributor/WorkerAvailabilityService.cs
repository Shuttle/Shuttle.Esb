using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Logging;

namespace Shuttle.Esb
{
    public class WorkerAvailabilityService : IWorkerAvailabilityService
    {
        private static readonly object _padlock = new object();

        private readonly ILog _log;

        private readonly Dictionary<string, List<AvailableWorker>> _workers =
            new Dictionary<string, List<AvailableWorker>>();

        public WorkerAvailabilityService()
        {
            _log = Log.For(this);
        }

        public AvailableWorker GetAvailableWorker()
        {
            lock (_padlock)
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
            lock (_padlock)
            {
                GetAvailableWorkers(message.Uri).Add(new AvailableWorker(message));
            }

            if (_log.IsTraceEnabled)
            {
                _log.Trace($"AvailableWorker: {message.Uri}");
            }
        }

        public void ReturnAvailableWorker(AvailableWorker availableWorker)
        {
            if (availableWorker == null)
            {
                return;
            }

            lock (_padlock)
            {
                GetAvailableWorkers(availableWorker.Uri).Add(availableWorker);
            }
        }

        public void WorkerStarted(WorkerStartedEvent message)
        {
            lock (_padlock)
            {
                _workers[message.Uri] = GetAvailableWorkers(message.Uri)
                    .Where(availableWorker => availableWorker.WorkerSendDate < message.DateStarted)
                    .ToList();
            }
        }

        public void RemoveByThread(WorkerThreadAvailableCommand message)
        {
            lock (_padlock)
            {
                GetAvailableWorkers(message.Uri)
                    .RemoveAll(candidate => candidate.ManagedThreadId == message.ManagedThreadId);
            }
        }

        private List<AvailableWorker> GetAvailableWorkers(string uri)
        {
            if (!_workers.TryGetValue(uri, out var worker))
            {
                worker = new List<AvailableWorker>();
                _workers.Add(uri, worker);
            }

            return worker;
        }
    }
}