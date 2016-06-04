using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class MessageNotHandledEventArgs : PipelineEventEventArgs
	{
		public IQueue WorkQueue { get; private set; }
		public IQueue ErrorQueue { get; private set; }
		public TransportMessage TransportMessage { get; private set; }
		public object Message { get; private set; }

		public MessageNotHandledEventArgs(IPipelineEvent pipelineEvent, IQueue workQueue, IQueue errorQueue,
			TransportMessage transportMessage, object message)
			: base(pipelineEvent)
		{
			Guard.AgainstNull(pipelineEvent, "pipelineEvent");
			Guard.AgainstNull(workQueue, "workQueue");
			Guard.AgainstNull(errorQueue, "errorQueue");
			Guard.AgainstNull(transportMessage, "transportMessage");
			Guard.AgainstNull(message, "message");

			WorkQueue = workQueue;
			ErrorQueue = errorQueue;
			TransportMessage = transportMessage;
			Message = message;
		}
	}
}