using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class OnGetMessage : PipelineEvent
	{
	}

	public class OnAfterGetMessage : PipelineEvent
	{
	}

	public class OnDeserializeTransportMessage : PipelineEvent
	{
	}

	public class OnAfterDeserializeTransportMessage : PipelineEvent
	{
	}

	public class OnDecryptMessage : PipelineEvent
	{
	}

	public class OnAfterDecryptMessage : PipelineEvent
	{
	}

	public class OnDeserializeMessage : PipelineEvent
	{
	}

	public class OnAfterDeserializeMessage : PipelineEvent
	{
	}

	public class OnAssessMessageHandling : PipelineEvent
	{
	}

	public class OnAfterAssessMessageHandling : PipelineEvent
	{
	}

	public class OnProcessIdempotenceMessage : PipelineEvent
	{
	}

	public class OnHandleMessage : PipelineEvent
	{
	}

	public class OnAfterHandleMessage : PipelineEvent
	{
	}

	public class OnIdempotenceMessageHandled : PipelineEvent
	{
	}

	public class OnRemoveJournalMessage : PipelineEvent
	{
	}


	public class OnAcknowledgeMessage : PipelineEvent
	{
	}

	public class OnAfterAcknowledgeMessage : PipelineEvent
	{
	}

	public class OnSendDeferred : PipelineEvent
	{
	}

	public class OnAfterSendDeferred : PipelineEvent
	{
	}

	public class OnProcessDeferredMessage : PipelineEvent
	{
	}

	public class OnAfterProcessDeferredMessage : PipelineEvent
	{
	}

	public class OnDecompressMessage : PipelineEvent
	{
	}

	public class OnAfterDecompressMessage : PipelineEvent
	{
	}
}