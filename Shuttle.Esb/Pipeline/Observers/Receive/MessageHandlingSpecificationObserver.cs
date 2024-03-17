using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IMessageHandlingSpecificationObserver : IPipelineObserver<OnEvaluateMessageHandling>
    {
    }

    public class MessageHandlingSpecificationObserver : IMessageHandlingSpecificationObserver
    {
        private readonly IMessageHandlingSpecification _messageHandlingSpecification;

        public MessageHandlingSpecificationObserver(IMessageHandlingSpecification messageHandlingSpecification)
        {
            Guard.AgainstNull(messageHandlingSpecification, nameof(messageHandlingSpecification));

            _messageHandlingSpecification = messageHandlingSpecification;
        }

        public void Execute(OnEvaluateMessageHandling pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            var state = pipelineEvent.Pipeline.State;

            if (_messageHandlingSpecification.IsSatisfiedBy(pipelineEvent))
            {
                return;
            }

            state.SetProcessingStatus(ProcessingStatus.Ignore);
        }

        public async Task ExecuteAsync(OnEvaluateMessageHandling pipelineEvent)
        {
            Execute(pipelineEvent);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}