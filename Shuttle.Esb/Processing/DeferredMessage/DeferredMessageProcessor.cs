using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public class DeferredMessageProcessor : IDeferredMessageProcessor
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly IPipelineFactory _pipelineFactory;
    private readonly ServiceBusOptions _serviceBusOptions;
    private Guid _checkpointMessageId = Guid.Empty;
    private DateTime _ignoreTillDate = DateTime.MaxValue.ToUniversalTime();
    private DateTime _nextProcessingDateTime = DateTime.MinValue.ToUniversalTime();

    public DeferredMessageProcessor(IOptions<ServiceBusOptions> serviceBusOptions, IPipelineFactory pipelineFactory)
    {
        _serviceBusOptions = Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
    }

    public event EventHandler<DeferredMessageProcessingAdjustedEventArgs>? DeferredMessageProcessingAdjusted;
    public event EventHandler<DeferredMessageProcessingHaltedEventArgs>? DeferredMessageProcessingHalted;

    private void AdjustNextProcessingDateTime(DateTime dateTime)
    {
        _nextProcessingDateTime = dateTime;

        DeferredMessageProcessingAdjusted?.Invoke(this, new(_nextProcessingDateTime));
    }

    public async Task ExecuteAsync(IProcessorThreadContext _, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_serviceBusOptions.Inbox.DeferredQueueUri))
        {
            return;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (DateTime.UtcNow < _nextProcessingDateTime)
            {
                try
                {
                    await Task.Delay(_serviceBusOptions.Inbox.DeferredMessageProcessorWaitInterval, cancellationToken).ConfigureAwait(false);
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

                await pipeline.ExecuteAsync(cancellationToken).ConfigureAwait(false);

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

                DeferredMessageProcessingHalted?.Invoke(this, new(_nextProcessingDateTime));
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

    public async Task MessageDeferredAsync(DateTime ignoreTillDate)
    {
        await _lock.WaitAsync(CancellationToken.None).ConfigureAwait(false);

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