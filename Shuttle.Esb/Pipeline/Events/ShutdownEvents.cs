using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class OnStopping : PipelineEvent
    {
    }

    public class OnStopped : PipelineEvent
    {
    }

    public class OnDisposeBrokerEndpoints : PipelineEvent
    {
    }

    public class OnAfterDisposeBrokerEndpoints : PipelineEvent
    {
    }
}