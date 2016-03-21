using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultMessageHandlingAssessor : IMessageHandlingAssessor
    {
        private readonly List<Func<PipelineEvent, bool>> _assessors = new List<Func<PipelineEvent, bool>>();
        private readonly List<ISpecification<PipelineEvent>> _specifications = new List<ISpecification<PipelineEvent>>();

        public bool IsSatisfiedBy(PipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, "pipelineEvent");

            return _assessors.All(assessor => assessor.Invoke(pipelineEvent))
                   &&
                   _specifications.All(specification => specification.IsSatisfiedBy(pipelineEvent));
        }

        public void RegisterAssessor(Func<PipelineEvent, bool> assessor)
        {
            Guard.AgainstNull(assessor, "assessor");

            _assessors.Add(assessor);
        }

        public void RegisterAssessor(ISpecification<PipelineEvent> specification)
        {
            Guard.AgainstNull(specification, "specification");

            _specifications.Add(specification);
        }
    }
}