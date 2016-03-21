using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class AcknowledgeMessageObserver :
		IPipelineObserver<OnAcknowledgeMessage>
	{
		private readonly ILog _log;

		public AcknowledgeMessageObserver()
		{
			_log = Log.For(this);
		}

		public void Execute(OnAcknowledgeMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;

			if (pipelineEvent.Pipeline.Exception != null && !state.GetTransactionComplete())
			{
				return;
			}

			state.GetWorkQueue().Acknowledge(state.GetReceivedMessage().AcknowledgementToken);
		}
	}
}
