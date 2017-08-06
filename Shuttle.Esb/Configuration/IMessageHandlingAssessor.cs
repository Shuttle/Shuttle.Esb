using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public interface IMessageHandlingAssessor : ISpecification<IPipelineEvent>
    {
        void RegisterAssessor(Func<IPipelineEvent, bool> assessor);
        void RegisterAssessor(ISpecification<IPipelineEvent> specification);
    }
}