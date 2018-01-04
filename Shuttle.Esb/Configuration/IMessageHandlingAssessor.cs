using System;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Specification;

namespace Shuttle.Esb
{
    public interface IMessageHandlingAssessor : ISpecification<IPipelineEvent>
    {
        void RegisterAssessor(Func<IPipelineEvent, bool> assessor);
        void RegisterAssessor(ISpecification<IPipelineEvent> specification);
    }
}