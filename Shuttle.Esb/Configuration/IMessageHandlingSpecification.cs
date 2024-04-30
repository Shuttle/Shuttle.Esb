using System;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Specification;

namespace Shuttle.Esb
{
    public interface IMessageHandlingSpecification : ISpecification<IPipelineEvent>
    {
        void Add(Func<IPipelineEvent, bool> assessor);
        void Add(ISpecification<IPipelineEvent> specification);
    }
}