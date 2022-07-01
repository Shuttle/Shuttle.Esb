using System;

namespace Shuttle.Esb
{
    public class AvailableWorker
    {
        public AvailableWorker(WorkerThreadAvailableCommand command)
        {
            Identifier = command.Identifier;
            Uri = command.Uri;
            ManagedThreadId = command.ManagedThreadId;
            WorkerSendDate = command.DateSent;
        }

        public Guid Identifier { get; }
        public string Uri { get; }
        public int ManagedThreadId { get; }
        public DateTime WorkerSendDate { get; }
    }
}