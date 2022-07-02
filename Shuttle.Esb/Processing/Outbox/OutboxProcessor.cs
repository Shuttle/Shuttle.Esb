using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class OutboxProcessor : QueueProcessor<OutboxPipeline>
    {
        public OutboxProcessor(IThreadActivity threadActivity, IPipelineFactory pipelineFactory)
            : base(threadActivity, pipelineFactory)
        {
        }
    }
}