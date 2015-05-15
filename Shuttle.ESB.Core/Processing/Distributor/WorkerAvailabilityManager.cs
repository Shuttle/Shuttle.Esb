using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class WorkerAvailabilityManager : IWorkerAvailabilityManager
	{
		private static readonly object _padlock = new object();

		private List<AvailableWorker> _availableWorkers = new List<AvailableWorker>();

		private readonly ILog _log;

		public WorkerAvailabilityManager()
		{
			_log = Log.For(this);
		}

		public AvailableWorker GetAvailableWorker()
		{
			lock (_padlock)
			{
				if (_availableWorkers.Count == 0)
				{
					return null;
				}

				var result = _availableWorkers[0];

				_availableWorkers.RemoveAt(0);

				return result;
			}
		}

		public void WorkerAvailable(WorkerThreadAvailableCommand message)
		{
			lock (_padlock)
			{
				_availableWorkers.Add(new AvailableWorker(message));
			}

			if (_log.IsTraceEnabled)
			{
				_log.Trace(string.Format("AvailableWorker: {0}", message.InboxWorkQueueUri));
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
				_availableWorkers.Add(availableWorker);
			}
		}

		public void WorkerStarted(WorkerStartedEvent message)
		{
			lock (_padlock)
			{
				var result = new List<AvailableWorker>();

				foreach (var availableWorker in _availableWorkers)
				{
					if (
						!(availableWorker.InboxWorkQueueUri.Equals(message.InboxWorkQueueUri,
						                                           StringComparison.InvariantCultureIgnoreCase) &&
						  availableWorker.WorkerSendDate < message.DateStarted))
					{
						result.Add(availableWorker);
					}
				}

				_availableWorkers = result;
			}
		}
	}
}