using Shuttle.Core.Pipelines;

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

    public class OnConfigureBrokerEndpoints : PipelineEvent
    {
    }

    public class OnAfterConfigureBrokerEndpoints : PipelineEvent
    {
    }

    public class OnCreatePhysicalBrokerEndpoints : PipelineEvent
    {
    }

    public class OnAfterCreatePhysicalBrokerEndpoints : PipelineEvent
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