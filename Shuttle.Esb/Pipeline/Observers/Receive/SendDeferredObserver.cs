﻿using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransactionScope;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface ISendDeferredObserver :
        IPipelineObserver<OnSendDeferred>,
        IPipelineObserver<OnAfterSendDeferred>
    {
    }

    public class SendDeferredObserver : ISendDeferredObserver
    {
        private readonly IIdempotenceService _idempotenceService;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly ISerializer _serializer;

        public SendDeferredObserver(IPipelineFactory pipelineFactory, ISerializer serializer,
            IIdempotenceService idempotenceService)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(serializer, nameof(serializer));
            Guard.AgainstNull(idempotenceService, nameof(idempotenceService));

            _pipelineFactory = pipelineFactory;
            _serializer = serializer;
            _idempotenceService = idempotenceService;
        }

        public void Execute(OnSendDeferred pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnSendDeferred pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        public void Execute(OnAfterSendDeferred pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAfterSendDeferred pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnAfterSendDeferred pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore ||
                (
                    pipelineEvent.Pipeline.Exception != null && !state.GetTransactionScopeCompleted()
                ))
            {
                return;
            }

            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);

            if (sync)
            {
                _idempotenceService.ProcessingCompleted(transportMessage);
            }
            else
            {
                await _idempotenceService.ProcessingCompletedAsync(transportMessage).ConfigureAwait(false);
            }
        }

        private async Task ExecuteAsync(OnSendDeferred pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);

            var deferredMessages = sync
                ? _idempotenceService.GetDeferredMessages(transportMessage)
                : await _idempotenceService.GetDeferredMessagesAsync(transportMessage).ConfigureAwait(false);

            foreach (var stream in deferredMessages)
            {
                var deferredTransportMessage = sync
                    ? (TransportMessage)_serializer.Deserialize(typeof(TransportMessage), stream)
                    : (TransportMessage)await _serializer.DeserializeAsync(typeof(TransportMessage), stream).ConfigureAwait(false);

                var messagePipeline = _pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

                try
                {
                    if (sync)
                    {
                        messagePipeline.Execute(deferredTransportMessage, null);
                    }
                    else
                    {
                        await messagePipeline.ExecuteAsync(deferredTransportMessage, null).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _pipelineFactory.ReleasePipeline(messagePipeline);
                }

                if (sync)
                {
                    _idempotenceService.DeferredMessageSent(transportMessage, deferredTransportMessage);
                }
                else
                {
                    await _idempotenceService.DeferredMessageSentAsync(transportMessage, deferredTransportMessage).ConfigureAwait(false);
                }
            }
        }
    }
}