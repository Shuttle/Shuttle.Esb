using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DeserializationExceptionEventArgs : PipelineEventEventArgs
	{
		public IQueue WorkQueue { get; private set; }
		public IQueue ErrorQueue { get; private set; }
		public Exception Exception { get; private set; }

		public DeserializationExceptionEventArgs(IPipelineEvent pipelineEvent, IQueue workQueue, IQueue errorQueue,
			Exception exception)
			: base(pipelineEvent)
		{
			WorkQueue = workQueue;
			ErrorQueue = errorQueue;
			Exception = exception;
		}
	}
}