using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DeferredMessageProcessor : IDeferredMessageProcessor
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private readonly IPipelineFactory _pipelineFactory;
        private readonly ServiceBusOptions _serviceBusOptions;
        private Guid _checkpointMessageId = Guid.Empty;
        private DateTime _ignoreTillDate = DateTime.MaxValue.ToUniversalTime();
        private DateTime _nextProcessingDateTime = DateTime.MinValue.ToUniversalTime();

        public DeferredMessageProcessor(IOptions<ServiceBusOptions> serviceBusOptions, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            
            _serviceBusOptions = Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
        }

        public void Execute(CancellationToken cancellationToken)
        {
            ExecuteAsync(cancellationToken, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await ExecuteAsync(cancellationToken, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken, bool sync)
        {
            if (!_serviceBusOptions.HasDeferredQueue())
            {
                return;
            }

            if (sync)
            {
                _lock.Wait(cancellationToken);
            }
            else
            {
                await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            try
            {
                if (DateTime.UtcNow < _nextProcessingDateTime)
                {
                    try
                    {
                        if (sync)
                        {
                            Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).Wait(cancellationToken);
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
                        }
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

                    if (sync)
                    {
                        pipeline.Execute(cancellationToken);
                    }
                    else
                    {
                        await pipeline.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    }

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

                    DeferredMessageProcessingHalted?.Invoke(this, new DeferredMessageProcessingHaltedEventArgs(_nextProcessingDateTime));
                }
                finally
                {
                    _pipelineFactory.ReleasePipeline(pipeline);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        private void AdjustNextProcessingDateTime(DateTime dateTime)
        {
            _nextProcessingDateTime = dateTime;

            DeferredMessageProcessingAdjusted?.Invoke(this, new DeferredMessageProcessingAdjustedEventArgs(_nextProcessingDateTime));
        }

        public event EventHandler<DeferredMessageProcessingAdjustedEventArgs> DeferredMessageProcessingAdjusted;

        public event EventHandler<DeferredMessageProcessingHaltedEventArgs> DeferredMessageProcessingHalted;

        public void MessageDeferred(DateTime ignoreTillDate)
        {
            MessageDeferredAsync(ignoreTillDate, true).GetAwaiter().GetResult();
        }

        public async Task MessageDeferredAsync(DateTime ignoreTillDate)
        {
            await MessageDeferredAsync(ignoreTillDate, false).ConfigureAwait(false);
        }

        private async Task MessageDeferredAsync(DateTime ignoreTillDate, bool sync)
        {
            if (sync)
            {
                _lock.Wait(CancellationToken.None);
            }
            else
            {
                await _lock.WaitAsync(CancellationToken.None).ConfigureAwait(false);
            }

            try
            {
                if (ignoreTillDate.ToUniversalTime() < _nextProcessingDateTime)
                {
                    AdjustNextProcessingDateTime(ignoreTillDate.ToUniversalTime());
                }
            }
            finally
            {
                _lock.Release(); 
            }
        }
    }
}