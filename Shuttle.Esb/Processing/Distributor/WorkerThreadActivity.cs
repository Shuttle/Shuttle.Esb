using System;
using System.Threading;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class WorkerThreadActivity : IThreadActivity
	{
		private readonly Guid _identifier = Guid.NewGuid();
	    private readonly IServiceBus _bus;
	    private readonly IServiceBusConfiguration _configuration;
	    private readonly ThreadActivity _threadActivity;

		private readonly ILog _log;

		private DateTime _nextNotificationDate = DateTime.Now;

		public WorkerThreadActivity(IServiceBus bus, IServiceBusConfiguration configuration, ThreadActivity threadActivity)
		{
			Guard.AgainstNull(configuration, "configuration");
			Guard.AgainstNull(threadActivity, "threadActivity");

		    _bus = bus;
		    _configuration = configuration;
		    _threadActivity = threadActivity;

			_log = Log.For(this);
		}

		public void Waiting(IThreadState state)
		{
			if (ShouldNotifyDistributor())
			{
				_bus.Send(new WorkerThreadAvailableCommand
				{
					Identifier = _identifier,
					InboxWorkQueueUri = _configuration.Inbox.WorkQueue.Uri.ToString(),
					ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
					DateSent = DateTime.Now
				},
					c => c.WithRecipient(_configuration.Worker.DistributorControlInboxWorkQueue));

				if (_log.IsVerboseEnabled)
				{
					_log.Verbose(string.Format(EsbResources.DebugWorkerAvailable,
						_identifier,
						_configuration.Inbox.WorkQueue.Uri.Secured(),
						_configuration.Worker.DistributorControlInboxWorkQueue.Uri.Secured()));
				}

				_nextNotificationDate = DateTime.Now.AddSeconds(_configuration.Worker.ThreadAvailableNotificationIntervalSeconds);
			}

			_threadActivity.Waiting(state);
		}

		public void Working()
		{
			_nextNotificationDate = DateTime.Now;

			_threadActivity.Working();
		}

		private bool ShouldNotifyDistributor()
		{
			return _nextNotificationDate <= DateTime.Now;
		}
	}
}