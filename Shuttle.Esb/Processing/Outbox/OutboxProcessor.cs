using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class OutboxProcessor : QueueProcessor<OutboxPipeline>
    {
        public OutboxProcessor(IServiceBus bus, IThreadActivity threadActivity, IPipelineFactory pipelineFactory)
            : base(bus, threadActivity, pipelineFactory)
        {
        }
    }
}