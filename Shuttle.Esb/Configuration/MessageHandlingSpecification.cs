using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Specification;

namespace Shuttle.Esb;

public class MessageHandlingSpecification : IMessageHandlingSpecification
{
    private readonly List<Func<IPipelineContext, bool>> _specificationFunctions = new();

    private readonly List<ISpecification<IPipelineContext>> _specifications = new();

    public bool IsSatisfiedBy(IPipelineContext pipelineContext)
    {
        Guard.AgainstNull(pipelineContext);

        return _specificationFunctions.All(assessor => assessor.Invoke(pipelineContext))
               &&
               _specifications.All(specification => specification.IsSatisfiedBy(pipelineContext));
    }

    public void Add(Func<IPipelineContext, bool> assessor)
    {
        _specificationFunctions.Add(Guard.AgainstNull(assessor));
    }

    public void Add(ISpecification<IPipelineContext> specification)
    {
        _specifications.Add(Guard.AgainstNull(specification));
    }
}