using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public interface IMessageHandlingAssessor : ISpecification<PipelineEvent>
	{
		void RegisterAssessor(Func<PipelineEvent, bool> assessor);
		void RegisterAssessor(ISpecification<PipelineEvent> specification);
	}
}