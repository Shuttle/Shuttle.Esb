using System;

namespace Shuttle.Esb
{
    public class WorkerOptions
    {
        public string DistributorControlInboxWorkQueueUri { get; set; }
        public TimeSpan ThreadAvailableNotificationInterval { get; set; }
    }
}