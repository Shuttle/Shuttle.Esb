using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class OnInitializing : PipelineEvent
    {
    }

    public class OnCreatePhysicalQueues : PipelineEvent
    {
    }

    public class OnAfterCreatePhysicalQueues : PipelineEvent
    {
    }

    public class OnConfigureThreadPools : PipelineEvent
    {
    }

    public class OnAfterConfigureThreadPools : PipelineEvent
    {
    }
    
    public class OnStartThreadPools : PipelineEvent
    {
    }

    public class OnAfterStartThreadPools : PipelineEvent
    {
    }

    public class OnStarting : PipelineEvent
    {
    }

    public class OnStarted : PipelineEvent
    {
    }
}