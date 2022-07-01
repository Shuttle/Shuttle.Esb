using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IGetWorkMessageObserver : IPipelineObserver<OnGetMessage>
    {
    }

    public class GetWorkMessageObserver : IGetWorkMessageObserver
    {
        private readonly IServiceBusEvents _events;

        public GetWorkMessageObserver(IServiceBusEvents events)
        {
            Guard.AgainstNull(events, nameof(events));

            _events = events;
        }

        public void Execute(OnGetMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var queue = state.GetBrokerEndpoint();

            Guard.AgainstNull(queue, nameof(queue));

            var receivedMessage = queue.GetMessage();

            // Abort the pipeline if there is no message on the queue
            if (receivedMessage == null)
            {
                _events.OnBrokerEndpointEmpty(this, new BrokerEndpointEmptyEventArgs(pipelineEvent, queue));
                pipelineEvent.Pipeline.Abort();
            }
            else
            {
                state.SetProcessingStatus(ProcessingStatus.Active);
                state.SetWorking();
                state.SetReceivedMessage(receivedMessage);
            }
        }
    }
}