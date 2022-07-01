using System;

namespace Shuttle.Esb
{
    public class WorkerStartedEvent
    {
        public string Uri { get; set; }
        public DateTime DateStarted { get; set; }
    }
}