using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class OnInitializing : PipelineEvent
	{
	}

	public class OnCreateQueues : PipelineEvent
	{
	}

	public class OnAfterCreateQueues : PipelineEvent
	{
	}

	public class OnRegisterMessageHandlers : PipelineEvent
	{
	}

	public class OnAfterInitializeMessageHandlerFactory : PipelineEvent
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

	public class OnStartWorker : PipelineEvent
	{
	}

	public class OnAfterStartWorker : PipelineEvent
	{
	}

	public class OnStarting : PipelineEvent
	{
	}

	public class OnStarted : PipelineEvent
	{
	}
}