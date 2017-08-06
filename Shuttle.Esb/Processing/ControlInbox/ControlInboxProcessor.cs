using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ControlInboxProcessor : QueueProcessor<ControlInboxMessagePipeline>
    {
        public ControlInboxProcessor(IServiceBusEvents events, IThreadActivity threadActivity,
            IPipelineFactory pipelineFactory) :
            base(events, threadActivity, pipelineFactory)
        {
        }
    }
}