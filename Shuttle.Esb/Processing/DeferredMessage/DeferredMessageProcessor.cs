using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class DeferredMessageProcessor : IProcessor
	{
		private readonly object _messageDeferredLock = new object();
		private DateTime _nextDeferredProcessDate = DateTime.MinValue;
		private DateTime _ignoreTillDate = DateTime.MaxValue;
		private Guid _checkpointMessageId = Guid.Empty;

		private readonly IServiceBus _bus;
	    private readonly IPipelineFactory _pipelineFactory;
	    private readonly ILog _log;

		public DeferredMessageProcessor(IServiceBus bus, IPipelineFactory pipelineFactory)
		{
			Guard.AgainstNull(bus, "bus");
			Guard.AgainstNull(pipelineFactory, "pipelineFactory");

			_bus = bus;
		    _pipelineFactory = pipelineFactory;
		    _log = Log.For(this);
		}

		public void Execute(IThreadState state)
		{
			if (!ShouldProcessDeferred())
			{
				ThreadSleep.While(1000, state);

				return;
			}

			var pipeline = _pipelineFactory.GetPipeline<DeferredMessagePipeline>();

			try
			{
				pipeline.State.ResetWorking();

				pipeline.Execute();

				if (pipeline.State.GetWorking())
				{
					var transportMessage = pipeline.State.GetTransportMessage();

					if (transportMessage.IgnoreTillDate < _ignoreTillDate)
					{
						_ignoreTillDate = transportMessage.IgnoreTillDate;
					}

					if (!_checkpointMessageId.Equals(transportMessage.MessageId))
					{
						if (!Guid.Empty.Equals(_checkpointMessageId))
						{
							return;
						}

						_checkpointMessageId = transportMessage.MessageId;

						_log.Trace(string.Format(EsbResources.TraceDeferredCheckpointMessageId, transportMessage.MessageId));

						return;
					}
				}

				_nextDeferredProcessDate = _ignoreTillDate;
				_ignoreTillDate = DateTime.MaxValue;
				_checkpointMessageId = Guid.Empty;

			    _log.Trace(_nextDeferredProcessDate.Equals(DateTime.MaxValue)
			        ? EsbResources.TraceDeferredProcessingHalted
			        : string.Format(EsbResources.TraceDeferredProcessingReset, _nextDeferredProcessDate.ToString("O")));
			}
			finally
			{
				_pipelineFactory.ReleasePipeline(pipeline);
			}
		}

		public void MessageDeferred(DateTime ignoreTillDate)
		{
			lock (_messageDeferredLock)
			{
				if (ignoreTillDate < _nextDeferredProcessDate)
				{
					_nextDeferredProcessDate = ignoreTillDate;
				}
			}
		}

		private bool ShouldProcessDeferred()
		{
			return (DateTime.Now >= _nextDeferredProcessDate);
		}
	}
}