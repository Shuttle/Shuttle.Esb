namespace Shuttle.ESB.Core
{
    public class AssessMessageHandlingObserver : IPipelineObserver<OnAssessMessageHandling>
    {
        public void Execute(OnAssessMessageHandling pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var configuration = state.GetServiceBus().Configuration;

            if (configuration.MessageHandlingAssessor.IsSatisfiedBy(pipelineEvent))
            {
                return;
            }

            state.SetProcessingStatus(ProcessingStatus.Ignore);
        }
    }
}