using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ControlInboxProcessor : QueueProcessor<ControlInboxMessagePipeline>
    {
        public ControlInboxProcessor(IServiceBus bus, IThreadActivity threadActivity, IPipelineFactory pipelineFactory) :
            base(bus, threadActivity, pipelineFactory)
        {
        }
    }
}