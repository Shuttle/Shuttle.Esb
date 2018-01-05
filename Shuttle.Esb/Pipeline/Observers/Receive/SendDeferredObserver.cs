using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
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
        private readonly ILog _log;
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

            _log = Log.For(this);
        }

        public void Execute(OnAfterSendDeferred pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (pipelineEvent.Pipeline.Exception != null && !state.GetTransactionComplete())
            {
                return;
            }

            var transportMessage = state.GetTransportMessage();

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            try
            {
                _idempotenceService.ProcessingCompleted(transportMessage);
            }
            catch (Exception ex)
            {
                _idempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
            }
        }

        public void Execute(OnSendDeferred pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            var transportMessage = state.GetTransportMessage();

            try
            {
                foreach (var stream in _idempotenceService.GetDeferredMessages(transportMessage))
                {
                    var deferredTransportMessage =
                        (TransportMessage) _serializer.Deserialize(typeof(TransportMessage), stream);

                    var messagePipeline = _pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

                    try
                    {
                        messagePipeline.Execute(deferredTransportMessage, null);
                    }
                    finally
                    {
                        _pipelineFactory.ReleasePipeline(messagePipeline);
                    }

                    _idempotenceService.DeferredMessageSent(transportMessage, deferredTransportMessage);
                }
            }
            catch (Exception ex)
            {
                _idempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
            }
        }
    }
}