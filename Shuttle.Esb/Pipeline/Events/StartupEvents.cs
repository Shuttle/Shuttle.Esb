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

    public class OnConfigure : PipelineEvent
    {
    }

    public class OnAfterConfigure : PipelineEvent
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