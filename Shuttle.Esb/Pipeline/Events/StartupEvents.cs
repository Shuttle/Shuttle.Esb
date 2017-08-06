using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class OnInitializing : PipelineEvent
    {
    }

    public class OnConfigureUriResolver : PipelineEvent
    {
    }

    public class OnAfterConfigureUriResolver : PipelineEvent
    {
    }

    public class OnConfigureQueueManager : PipelineEvent
    {
    }

    public class OnAfterConfigureQueueManager : PipelineEvent
    {
    }

    public class OnConfigureQueues : PipelineEvent
    {
    }

    public class OnAfterConfigureQueues : PipelineEvent
    {
    }

    public class OnCreatePhysicalQueues : PipelineEvent
    {
    }

    public class OnAfterCreatePhysicalQueues : PipelineEvent
    {
    }

    public class OnConfigureMessageRouteProvider : PipelineEvent
    {
    }

    public class OnAfterConfigureMessageRouteProvider : PipelineEvent
    {
    }

    public class OnStartInboxProcessing : PipelineEvent
    {
    }

    public class OnAfterStartInboxProcessing : PipelineEvent
    {
    }

    public class OnStartControlInboxProcessing : PipelineEvent
    {
    }

    public class OnAfterStartControlInboxProcessing : PipelineEvent
    {
    }

    public class OnStartOutboxProcessing : PipelineEvent
    {
    }

    public class OnAfterStartOutboxProcessing : PipelineEvent
    {
    }

    public class OnStartDeferredMessageProcessing : PipelineEvent
    {
    }

    public class OnAfterStartDeferredMessageProcessing : PipelineEvent
    {
    }

    public class OnStarting : PipelineEvent
    {
    }

    public class OnStarted : PipelineEvent
    {
    }
}