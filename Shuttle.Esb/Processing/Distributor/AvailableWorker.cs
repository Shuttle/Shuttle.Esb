using System;

namespace Shuttle.Esb
{
    public class AvailableWorker
    {
        public AvailableWorker(WorkerThreadAvailableCommand command)
        {
            Identifier = command.Identifier;
            InboxWorkQueueUri = command.InboxWorkQueueUri;
            ManagedThreadId = command.ManagedThreadId;
            WorkerSendDate = command.DateSent;
        }

        public Guid Identifier { get; }
        public string InboxWorkQueueUri { get; }
        public int ManagedThreadId { get; }
        public DateTime WorkerSendDate { get; }
    }
}