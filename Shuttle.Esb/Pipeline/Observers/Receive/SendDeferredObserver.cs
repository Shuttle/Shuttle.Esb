using System.Threading.Tasks;
using Shuttle.Core.Contract;
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

        public async Task Execute(OnAfterSendDeferred pipelineEvent)
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

            _idempotenceService.ProcessingCompleted(transportMessage);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task Execute(OnSendDeferred pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            var transportMessage = state.GetTransportMessage();

            foreach (var stream in _idempotenceService.GetDeferredMessages(transportMessage))
            {
                var deferredTransportMessage =
                    (TransportMessage)await _serializer.Deserialize(typeof(TransportMessage), stream).ConfigureAwait(false);

                var messagePipeline = _pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

                try
                {
                    await messagePipeline.Execute(deferredTransportMessage, null).ConfigureAwait(false);
                }
                finally
                {
                    _pipelineFactory.ReleasePipeline(messagePipeline);
                }

                _idempotenceService.DeferredMessageSent(transportMessage, deferredTransportMessage);
            }
        }
    }
}