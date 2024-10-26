using System;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Specification;

namespace Shuttle.Esb;

public interface IMessageHandlingSpecification : ISpecification<IPipelineContext>
{
    void Add(Func<IPipelineContext, bool> assessor);
    void Add(ISpecification<IPipelineContext> specification);
}