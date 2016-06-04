using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class PipelineEventEventArgs
	{
		public PipelineEventEventArgs(IPipelineEvent pipelineEvent)
		{
			PipelineEvent = pipelineEvent;
		}

		public IPipelineEvent PipelineEvent { get; private set; }
	}
}