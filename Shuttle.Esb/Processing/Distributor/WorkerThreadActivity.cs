using System;
using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class WorkerThreadActivity : IThreadActivity
    {
        private readonly IServiceBus _serviceBus;
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly Guid _identifier = Guid.NewGuid();

        private readonly ThreadActivity _threadActivity;

        private DateTime _nextNotificationDate = DateTime.Now;

        public WorkerThreadActivity(IServiceBus serviceBus, IServiceBusConfiguration serviceBusConfiguration,
            ThreadActivity threadActivity)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(threadActivity, nameof(threadActivity));

            _serviceBus = serviceBus;
            _serviceBusConfiguration = serviceBusConfiguration;
            _threadActivity = threadActivity;
        }

        public void Waiting(CancellationToken cancellationToken)
        {
            if (ShouldNotifyDistributor())
            {
                _serviceBus.Send(new WorkerThreadAvailableCommand
                    {
                        Identifier = _identifier,
                        InboxWorkQueueUri = _serviceBusConfiguration.Inbox.WorkQueue.Uri.ToString(),
                        ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
                        DateSent = DateTime.Now
                    },
                    c => c.WithRecipient(_serviceBusConfiguration.Worker.DistributorControlInboxWorkQueue));

                _nextNotificationDate =
                    DateTime.Now.AddSeconds(_serviceBusConfiguration.Worker.ThreadAvailableNotificationIntervalSeconds);
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