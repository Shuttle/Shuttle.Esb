using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IAssessMessageHandlingObserver : IPipelineObserver<OnAssessMessageHandling>
    {
    }

    public class AssessMessageHandlingObserver : IAssessMessageHandlingObserver
    {
        private readonly IMessageHandlingAssessor _messageHandlingAssessor;

        public AssessMessageHandlingObserver(IMessageHandlingAssessor messageHandlingAssessor)
        {
            Guard.AgainstNull(messageHandlingAssessor, nameof(messageHandlingAssessor));

            _messageHandlingAssessor = messageHandlingAssessor;
        }

        public void Execute(OnAssessMessageHandling pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (_messageHandlingAssessor.IsSatisfiedBy(pipelineEvent))
            {
                return;
            }

            state.SetProcessingStatus(ProcessingStatus.Ignore);
        }
    }
}