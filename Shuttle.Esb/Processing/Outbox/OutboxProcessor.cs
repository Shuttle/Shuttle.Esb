using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class OutboxProcessor : QueueProcessor<OutboxPipeline>
    {
        public OutboxProcessor(IServiceBusEvents events, IThreadActivity threadActivity,
            IPipelineFactory pipelineFactory)
            : base(events, threadActivity, pipelineFactory)
        {
        }
    }
}