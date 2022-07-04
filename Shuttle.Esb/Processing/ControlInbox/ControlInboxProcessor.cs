using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class ControlInboxProcessor : QueueProcessor<ControlInboxMessagePipeline>
    {
        public ControlInboxProcessor(IThreadActivity threadActivity, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity) :
            base(threadActivity, pipelineFactory, pipelineThreadActivity)
        {
        }
    }
}