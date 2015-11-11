namespace Shuttle.ESB.Core
{
    public class AssessMessageHandlingObserver : IPipelineObserver<OnAssessMessageHandling>
    {
        public void Execute(OnAssessMessageHandling pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var configuration = state.GetServiceBus().Configuration;

            state.SetShouldProcess(configuration.MessageHandlingAssessor.IsSatisfiedBy(pipelineEvent));
        }
    }
}