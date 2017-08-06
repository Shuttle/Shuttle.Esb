using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class AssessMessageHandlingObserver : IPipelineObserver<OnAssessMessageHandling>
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