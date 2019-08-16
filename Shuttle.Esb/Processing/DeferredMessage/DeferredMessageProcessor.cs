using System;
using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class DeferredMessageProcessor : IProcessor
    {
        private readonly ILog _log;
        private readonly object _messageDeferredLock = new object();

        private readonly IPipelineFactory _pipelineFactory;
        private Guid _checkpointMessageId = Guid.Empty;
        private DateTime _ignoreTillDate = DateTime.MaxValue;
        private DateTime _nextDeferredProcessDate = DateTime.MinValue;

        public DeferredMessageProcessor(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _pipelineFactory = pipelineFactory;
            _log = Log.For(this);
        }

        public void Execute(CancellationToken cancellationToken)
        {
            if (!ShouldProcessDeferred())
            {
                ThreadSleep.While(1000, cancellationToken);

                return;
            }

            var pipeline = _pipelineFactory.GetPipeline<DeferredMessagePipeline>();

            try
            {
                pipeline.State.ResetWorking();
                pipeline.State.SetDeferredMessageReturned(false);
                pipeline.State.SetTransportMessage(null);

                pipeline.Execute();

                var transportMessage = pipeline.State.GetTransportMessage();

                if (pipeline.State.GetDeferredMessageReturned())
                {
                    if (transportMessage != null &&
                        transportMessage.MessageId.Equals(_checkpointMessageId))
                    {
                        _checkpointMessageId = Guid.Empty;
                    }

                    return;
                }

                if (pipeline.State.GetWorking() && transportMessage != null)
                {
                    if (transportMessage.IgnoreTillDate < _ignoreTillDate)
                    {
                        _ignoreTillDate = transportMessage.IgnoreTillDate;
                    }

                    if (!_checkpointMessageId.Equals(transportMessage.MessageId))
                    {
                        if (!_checkpointMessageId.Equals(Guid.Empty))
                        {
                            return;
                        }

                        _checkpointMessageId = transportMessage.MessageId;

                        _log.Trace(string.Format(Resources.TraceDeferredCheckpointMessageId,
                            transportMessage.MessageId));

                        return;
                    }
                }

                lock (_messageDeferredLock)
                {
                    _nextDeferredProcessDate = _ignoreTillDate;
                }

                _ignoreTillDate = DateTime.MaxValue;
                _checkpointMessageId = Guid.Empty;

                _log.Trace(_nextDeferredProcessDate.Equals(DateTime.MaxValue)
                    ? Resources.TraceDeferredProcessingHalted
                    : string.Format(Resources.TraceDeferredProcessingReset, _nextDeferredProcessDate.ToString("O")));
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
            lock (_messageDeferredLock)
            {
                return DateTime.Now >= _nextDeferredProcessDate;
            }
        }
    }
}