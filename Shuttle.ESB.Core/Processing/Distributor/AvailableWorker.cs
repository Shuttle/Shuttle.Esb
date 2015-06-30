using System;

namespace Shuttle.ESB.Core
{
    public class AvailableWorker
    {
        public Guid Identifier { get; private set; }
        public string InboxWorkQueueUri { get; private set; }
        public int ManagedThreadId { get; private set; }
        public DateTime WorkerSendDate { get; private set; }

        public AvailableWorker(WorkerThreadAvailableCommand command)
        {
            Identifier = command.Identifier;
            InboxWorkQueueUri = command.InboxWorkQueueUri;
            ManagedThreadId = command.ManagedThreadId;
            WorkerSendDate = command.DateSent;
        }
    }
}