using System;

namespace Shuttle.Esb
{
	public class WorkerStartedEvent
	{
		public string InboxWorkQueueUri { get; set; }
		public DateTime DateStarted { get; set; }
	}
}