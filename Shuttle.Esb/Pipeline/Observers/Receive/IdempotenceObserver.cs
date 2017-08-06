using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class IdempotenceObserver :
        IPipelineObserver<OnProcessIdempotenceMessage>,
        IPipelineObserver<OnIdempotenceMessageHandled>
    {
        private readonly IIdempotenceService _idempotenceService;
        private readonly ILog _log;

        public IdempotenceObserver(IIdempotenceService idempotenceService)
        {
            Guard.AgainstNull(idempotenceService, nameof(idempotenceService));

            _idempotenceService = idempotenceService;
            _log = Log.For(this);
        }

        public void Execute(OnIdempotenceMessageHandled pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            _idempotenceService.MessageHandled(transportMessage);
        }

        public void Execute(OnProcessIdempotenceMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            var transportMessage = state.GetTransportMessage();

            try
            {
                state.SetProcessingStatus(_idempotenceService.ProcessingStatus(transportMessage));
            }
            catch (Exception ex)
            {
                _idempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
            }
        }
    }
}