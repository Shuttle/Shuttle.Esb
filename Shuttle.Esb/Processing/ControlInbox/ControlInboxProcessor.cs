using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class ControlInboxProcessor : BrokerEndpointProcessor<ControlInboxMessagePipeline>
    {
        public ControlInboxProcessor(IServiceBusEvents events, IThreadActivity threadActivity,
            IPipelineFactory pipelineFactory) :
            base(events, threadActivity, pipelineFactory)
        {
        }
    }
}