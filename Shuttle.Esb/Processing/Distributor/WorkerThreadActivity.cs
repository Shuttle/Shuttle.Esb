using System;
using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class WorkerThreadActivity : IThreadActivity
    {
        private readonly IServiceBus _bus;
        private readonly IServiceBusConfiguration _configuration;
        private readonly Guid _identifier = Guid.NewGuid();

        private readonly ILog _log;
        private readonly ThreadActivity _threadActivity;

        private DateTime _nextNotificationDate = DateTime.Now;

        public WorkerThreadActivity(IServiceBus bus, IServiceBusConfiguration configuration,
            ThreadActivity threadActivity)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(threadActivity, nameof(threadActivity));

            _bus = bus;
            _configuration = configuration;
            _threadActivity = threadActivity;

            _log = Log.For(this);
        }

        public void Waiting(CancellationToken cancellationToken)
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
                    _log.Verbose(string.Format(Resources.DebugWorkerAvailable,
                        _identifier,
                        _configuration.Inbox.WorkQueue.Uri.Secured(),
                        _configuration.Worker.DistributorControlInboxWorkQueue.Uri.Secured()));
                }

                _nextNotificationDate =
                    DateTime.Now.AddSeconds(_configuration.Worker.ThreadAvailableNotificationIntervalSeconds);
            }

            _threadActivity.Waiting(cancellationToken);
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