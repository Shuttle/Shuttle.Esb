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

        public void Execute(OnIdempotenceMessageHandled pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnIdempotenceMessageHandled pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false);
        }

        public void Execute(OnProcessIdempotenceMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnProcessIdempotenceMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false);
        }

        private async Task ExecuteAsync(OnIdempotenceMessageHandled pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);

            if (sync)
            {
                _idempotenceService.MessageHandled(transportMessage);
            }
            else
            {
                await _idempotenceService.MessageHandledAsync(transportMessage);
            }
        }

        private async Task ExecuteAsync(OnProcessIdempotenceMessage pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            state.SetProcessingStatus(sync
                ? _idempotenceService.ProcessingStatus(Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage))
                : await _idempotenceService.ProcessingStatusAsync(Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage)));
        }
    }
}