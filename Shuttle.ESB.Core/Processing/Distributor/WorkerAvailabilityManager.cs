using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class WorkerAvailabilityManager : IWorkerAvailabilityManager
	{
		private static readonly object _padlock = new object();

		private readonly Dictionary<string, List<AvailableWorker>> _workers = new Dictionary<string, List<AvailableWorker>>();

		private readonly ILog _log;

		public WorkerAvailabilityManager()
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
				GetAvailableWorkers(message.InboxWorkQueueUri).Add(new AvailableWorker(message));
			}

			if (_log.IsTraceEnabled)
			{
				_log.Trace(string.Format("AvailableWorker: {0}", message.InboxWorkQueueUri));
			}
		}

		private List<AvailableWorker> GetAvailableWorkers(string inboxWorkQueueUri)
		{
			if (!_workers.ContainsKey(inboxWorkQueueUri))
			{
				_workers.Add(inboxWorkQueueUri, new List<AvailableWorker>());
			}

			return _workers[inboxWorkQueueUri];
		}

		public void ReturnAvailableWorker(AvailableWorker availableWorker)
		{
			if (availableWorker == null)
			{
				return;
			}

			lock (_padlock)
			{
				GetAvailableWorkers(availableWorker.InboxWorkQueueUri).Add(availableWorker);
			}
		}

		public void WorkerStarted(WorkerStartedEvent message)
		{
			lock (_padlock)
			{
				_workers[message.InboxWorkQueueUri] = GetAvailableWorkers(message.InboxWorkQueueUri)
					.Where(availableWorker => availableWorker.WorkerSendDate < message.DateStarted)
					.ToList();
			}
		}

		public void RemoveByThread(WorkerThreadAvailableCommand message)
		{
			lock (_padlock)
			{
				GetAvailableWorkers(message.InboxWorkQueueUri)
					.RemoveAll(candidate => candidate.ManagedThreadId == message.ManagedThreadId);
			}
		}
	}
}