using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class QueueEmptyEventArgs : PipelineEventEventArgs
    {
        public QueueEmptyEventArgs(IPipelineEvent pipelineEvent, IQueue queue)
            : base(pipelineEvent)
        {
            Queue = queue;
        }

        public IQueue Queue { get; }
    }
}