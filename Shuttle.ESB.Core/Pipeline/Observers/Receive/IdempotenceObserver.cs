using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
    public class IdempotenceObserver :
        IPipelineObserver<OnProcessIdempotenceMessage>,
        IPipelineObserver<OnIdempotenceMessageHandled>
    {
        private readonly ILog _log;

        public IdempotenceObserver()
        {
            _log = Log.For(this);
        }

        public void Execute(OnIdempotenceMessageHandled pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var bus = state.GetServiceBus();
            var transportMessage = state.GetTransportMessage();

            if (!bus.Configuration.HasIdempotenceService || state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            bus.Configuration.IdempotenceService.MessageHandled(transportMessage);
        }

        public void Execute(OnProcessIdempotenceMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var bus = state.GetServiceBus();

            if (!bus.Configuration.HasIdempotenceService || state.GetProcessingStatus() == ProcessingStatus.Ignore)
            {
                return;
            }

            var transportMessage = state.GetTransportMessage();

            try
            {
                state.SetProcessingStatus(bus.Configuration.IdempotenceService.ProcessingStatus(transportMessage));
            }
            catch (Exception ex)
            {
                bus.Configuration.IdempotenceService.AccessException(_log, ex, pipelineEvent.Pipeline);
            }
        }
    }
}