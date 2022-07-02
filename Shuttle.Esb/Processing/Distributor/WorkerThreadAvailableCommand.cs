using System;

namespace Shuttle.Esb
{
    public class WorkerThreadAvailableCommand
    {
        public Guid Identifier { get; set; }
        public string InboxWorkQueueUri { get; set; }
        public int ManagedThreadId { get; set; }
        public DateTime DateSent { get; set; }
    }
}