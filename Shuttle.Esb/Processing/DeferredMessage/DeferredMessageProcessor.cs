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
        private DateTime _nextDeferredProcessDate = DateTime.MinValue.ToUniversalTime();
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
                if (DateTime.UtcNow < _nextDeferredProcessDate)
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

                    lock (_lock)
                    {
                        _nextDeferredProcessDate =
                            DateTime.UtcNow.Add(_serviceBusOptions.Inbox.DeferredMessageProcessorResetInterval);
                    }

                    DeferredMessageProcessingHalted.Invoke(this,
                        new DeferredMessageProcessingHaltedEventArgs(_nextDeferredProcessDate));
                }
                finally
                {
                    _pipelineFactory.ReleasePipeline(pipeline);
                }
            }
        }

        public void MessageDeferred(DateTime ignoreTillDate)
        {
            lock (_lock)
            {
                if (ignoreTillDate.ToUniversalTime() < _nextDeferredProcessDate)
                {
                    _nextDeferredProcessDate = ignoreTillDate.ToUniversalTime();
                }
            }
        }

        public event EventHandler<DeferredMessageProcessingHaltedEventArgs> DeferredMessageProcessingHalted = delegate
        {
        };
    }
}