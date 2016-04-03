using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ProcessDeferredMessageObserver : IPipelineObserver<OnProcessDeferredMessage>
    {
		public void Execute(OnProcessDeferredMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetTransportMessage();
			var receivedMessage = state.GetReceivedMessage();
			var workQueue = state.GetWorkQueue();
			var deferredQueue = state.GetDeferredQueue();

			Guard.AgainstNull(transportMessage, "transportMessage");
			Guard.AgainstNull(receivedMessage, "receivedMessage");
			Guard.AgainstNull(workQueue, "workQueue");
			Guard.AgainstNull(deferredQueue, "deferredQueue");

			if (transportMessage.IsIgnoring())
			{
				deferredQueue.Release(receivedMessage.AcknowledgementToken);

				return;
			}

			workQueue.Enqueue(transportMessage.MessageId, receivedMessage.Stream);			
			deferredQueue.Acknowledge(receivedMessage.AcknowledgementToken);
		}
    }
}