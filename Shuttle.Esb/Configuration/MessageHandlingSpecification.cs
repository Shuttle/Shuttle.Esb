using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Specification;

namespace Shuttle.Esb
{
    public class MessageHandlingSpecification : IMessageHandlingSpecification
    {
        private readonly List<Func<IPipelineEvent, bool>> _specificationFunctions = new List<Func<IPipelineEvent, bool>>();

        private readonly List<ISpecification<IPipelineEvent>> _specifications = new List<ISpecification<IPipelineEvent>>();

        public bool IsSatisfiedBy(IPipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            return _specificationFunctions.All(assessor => assessor.Invoke(pipelineEvent))
                   &&
                   _specifications.All(specification => specification.IsSatisfiedBy(pipelineEvent));
        }

        public void Add(Func<IPipelineEvent, bool> assessor)
        {
            Guard.AgainstNull(assessor, nameof(assessor));

            _specificationFunctions.Add(assessor);
        }

        public void Add(ISpecification<IPipelineEvent> specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            _specifications.Add(specification);
        }
    }
}