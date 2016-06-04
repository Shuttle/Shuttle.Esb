using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class PipelineEventArgs : EventArgs
	{
		public IPipeline Pipeline { get; private set; }

		public PipelineEventArgs(IPipeline pipeline)
		{
			Pipeline = pipeline;
		}
	}
}