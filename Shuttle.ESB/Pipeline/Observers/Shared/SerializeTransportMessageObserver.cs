using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class SerializeTransportMessageObserver : IPipelineObserver<OnSerializeTransportMessage>
	{
		public void Execute(OnSerializeTransportMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetTransportMessage();

			Guard.AgainstNull(transportMessage, "transportMessage");

			state.SetTransportMessageStream(state.GetServiceBus().Configuration.Serializer.Serialize(transportMessage));
		}
	}
}