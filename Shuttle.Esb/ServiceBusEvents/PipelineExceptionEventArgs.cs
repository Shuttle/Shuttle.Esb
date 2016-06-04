using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class PipelineExceptionEventArgs : EventArgs
	{
		public IPipeline Pipeline { get; private set; }

		public PipelineExceptionEventArgs(IPipeline pipeline)
		{
			Pipeline = pipeline;
		}
	}
}