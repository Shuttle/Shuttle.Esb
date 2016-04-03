using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class CompressMessageObserver : IPipelineObserver<OnCompressMessage>
	{
		public void Execute(OnCompressMessage pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetTransportMessage();

			if (!transportMessage.CompressionEnabled())
			{
				return;
			}

			var algorithm =
				state.GetServiceBus().Configuration.FindCompressionAlgorithm(transportMessage.CompressionAlgorithm);

			Guard.Against<InvalidOperationException>(algorithm == null, string.Format(EsbResources.CompressionAlgorithmException, transportMessage.CompressionAlgorithm));

			transportMessage.Message = algorithm.Compress(transportMessage.Message);
		}
	}
}