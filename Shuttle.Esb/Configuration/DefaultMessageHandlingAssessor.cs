using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultMessageHandlingAssessor : IMessageHandlingAssessor
    {
        private readonly List<Func<IPipelineEvent, bool>> _assessors = new List<Func<IPipelineEvent, bool>>();

        private readonly List<ISpecification<IPipelineEvent>> _specifications =
            new List<ISpecification<IPipelineEvent>>();

        public bool IsSatisfiedBy(IPipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            return _assessors.All(assessor => assessor.Invoke(pipelineEvent))
                   &&
                   _specifications.All(specification => specification.IsSatisfiedBy(pipelineEvent));
        }

        public void RegisterAssessor(Func<IPipelineEvent, bool> assessor)
        {
            Guard.AgainstNull(assessor, nameof(assessor));

            _assessors.Add(assessor);
        }

        public void RegisterAssessor(ISpecification<IPipelineEvent> specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            _specifications.Add(specification);
        }
    }
}