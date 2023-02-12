using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IIdempotenceObserver :
        IPipelineObserver<OnProcessIdempotenceMessage>,
        IPipelineObserver<OnIdempotenceMessageHandled>
    {
    }

    public class IdempotenceObserver : IIdempotenceObserver
    {
        private readonly IIdempotenceService _idempotenceService;

        public IdempotenceObserver(IIdempotenceService idempotenceService)
        {
            Guard.AgainstNull(idempotenceService, nameof(idempotenceService));

            _idempotenceService = idempotenceService;
        }

        public async Task Execute(OnIdempotenceMessageHandled pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetTransportMessage();

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            _idempotenceService.MessageHandled(transportMessage);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task Execute(OnProcessIdempotenceMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            state.SetProcessingStatus(_idempotenceService.ProcessingStatus(state.GetTransportMessage()));

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}