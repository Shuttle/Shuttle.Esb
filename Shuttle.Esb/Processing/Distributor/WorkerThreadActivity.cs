using System;
using System.Threading;
using Microsoft.Extensions.Options;
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

        private DateTime _nextNotificationDate = DateTime.UtcNow;
        private readonly ServiceBusOptions _serviceBusOptions;

        public WorkerThreadActivity(ServiceBusOptions serviceBusOptions, IServiceBus serviceBus, IServiceBusConfiguration serviceBusConfiguration,
            ThreadActivity threadActivity)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(threadActivity, nameof(threadActivity));

            _serviceBusOptions = serviceBusOptions;
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
                        DateSent = DateTime.UtcNow
                    },
                    builder => builder.WithRecipient(_serviceBusConfiguration.Worker.DistributorControlInboxWorkQueue));

                _nextNotificationDate = DateTime.UtcNow.Add(_serviceBusOptions.Worker.ThreadAvailableNotificationInterval);
            }

            _threadActivity.Waiting(cancellationToken);
        }

        public void Working()
        {
            _nextNotificationDate = DateTime.UtcNow;

            _threadActivity.Working();
        }

        private bool ShouldNotifyDistributor()
        {
            return _nextNotificationDate <= DateTime.UtcNow;
        }
    }
}