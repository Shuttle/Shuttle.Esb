using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class OnStopping : PipelineEvent
    {
    }

    public class OnStopped : PipelineEvent
    {
    }

    public class OnDisposeQueues : PipelineEvent
    {
    }

    public class OnAfterDisposeQueues : PipelineEvent
    {
    }
}