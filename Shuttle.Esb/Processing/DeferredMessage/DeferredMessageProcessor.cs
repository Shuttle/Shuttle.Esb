using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class DeferredMessageProcessor : IProcessor
    {
        private readonly object _lock = new object();

        private readonly IPipelineFactory _pipelineFactory;
        private Guid _checkpointMessageId = Guid.Empty;
        private DateTime _ignoreTillDate = DateTime.MaxValue.ToUniversalTime();
        private DateTime _nextProcessingDateTime = DateTime.MinValue.ToUniversalTime();
        private readonly ServiceBusOptions _serviceBusOptions;

        public DeferredMessageProcessor(ServiceBusOptions serviceBusOptions, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _serviceBusOptions = serviceBusOptions;
            _pipelineFactory = pipelineFactory;
        }

        public void Execute(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (DateTime.UtcNow < _nextProcessingDateTime)
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
                        if (transportMessage.IgnoreTillDate.ToUniversalTime() < _ignoreTillDate)
                        {
                            _ignoreTillDate = transportMessage.IgnoreTillDate.ToUniversalTime();
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

                    _checkpointMessageId = Guid.Empty;

                    if (_nextProcessingDateTime > DateTime.UtcNow)
                    {
                        return;
                    }

                    var nextProcessingDateTime = DateTime.UtcNow.Add(_serviceBusOptions.Inbox.DeferredMessageProcessorResetInterval);

                    AdjustNextProcessingDateTime(_ignoreTillDate < nextProcessingDateTime
                        ? _ignoreTillDate
                        : nextProcessingDateTime);

                    _ignoreTillDate = DateTime.MaxValue.ToUniversalTime();

                    DeferredMessageProcessingHalted.Invoke(this,
                        new DeferredMessageProcessingHaltedEventArgs(_nextProcessingDateTime));
                }
                finally
                {
                    _pipelineFactory.ReleasePipeline(pipeline);
                }
            }
        }

        private void AdjustNextProcessingDateTime(DateTime dateTime)
        {
            _nextProcessingDateTime = dateTime;

            DeferredMessageProcessingAdjusted(this,
                new DeferredMessageProcessingAdjustedEventArgs(_nextProcessingDateTime));
        }

        public void MessageDeferred(DateTime ignoreTillDate)
        {
            lock (_lock)
            {
                if (ignoreTillDate.ToUniversalTime() < _nextProcessingDateTime)
                {
                    AdjustNextProcessingDateTime(ignoreTillDate.ToUniversalTime());
                }
            }
        }

        public event EventHandler<DeferredMessageProcessingAdjustedEventArgs> DeferredMessageProcessingAdjusted= delegate
        {
        };

        public event EventHandler<DeferredMessageProcessingHaltedEventArgs> DeferredMessageProcessingHalted = delegate
        {
        };
    }
}