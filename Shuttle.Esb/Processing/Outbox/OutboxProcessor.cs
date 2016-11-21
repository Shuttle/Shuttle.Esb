using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class OutboxProcessor : QueueProcessor<OutboxPipeline>
    {
        public OutboxProcessor(IServiceBusEvents events, IThreadActivity threadActivity, IPipelineFactory pipelineFactory)
            : base(events, threadActivity, pipelineFactory)
        {
        }
    }
}