using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class DeferredMessageProcessor : IProcessor
    {
        private readonly object _messageDeferredLock = new object();

        private readonly IPipelineFactory _pipelineFactory;
        private Guid _checkpointMessageId = Guid.Empty;
        private DateTime _ignoreTillDate = DateTime.MaxValue;
        private DateTime _nextDeferredProcessDate = DateTime.MinValue;

        public DeferredMessageProcessor(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _pipelineFactory = pipelineFactory;
        }

        public void Execute(CancellationToken cancellationToken)
        {
            if (!ShouldProcessDeferred())
            {
                try
                {
                    Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).Wait(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }

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

                        return;
                    }
                }

                lock (_messageDeferredLock)
                {
                    _nextDeferredProcessDate = _ignoreTillDate;
                }

                _ignoreTillDate = DateTime.MaxValue;
                _checkpointMessageId = Guid.Empty;
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