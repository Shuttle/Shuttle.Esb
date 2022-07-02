using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class ControlInboxProcessor : QueueProcessor<ControlInboxMessagePipeline>
    {
        public ControlInboxProcessor(IThreadActivity threadActivity,
            IPipelineFactory pipelineFactory) :
            base(threadActivity, pipelineFactory)
        {
        }
    }
}