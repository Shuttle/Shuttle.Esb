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

        public async Task ExecuteAsync(OnIdempotenceMessageHandled pipelineEvent)
        {
            Execute(pipelineEvent);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public void Execute(OnProcessIdempotenceMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            if (state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            state.SetProcessingStatus(_idempotenceService.ProcessingStatus(state.GetTransportMessage()));
        }

        public async Task ExecuteAsync(OnProcessIdempotenceMessage pipelineEvent)
        {
            Execute(pipelineEvent);
            
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}