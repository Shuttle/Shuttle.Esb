using System;
using System.Diagnostics;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;
using Shuttle.Core.System;

namespace Shuttle.Esb
{
    public interface IDeserializeTransportMessageObserver : IPipelineObserver<OnDeserializeTransportMessage>
    {
    }

    public class DeserializeTransportMessageObserver : IDeserializeTransportMessageObserver
    {
        private readonly IServiceBusConfiguration _configuration;
        private readonly IServiceBusEvents _events;
        private readonly ILog _log;
        private readonly ISerializer _serializer;
        private readonly IEnvironmentService _environmentService;
        private readonly IProcessService _processService;

        public DeserializeTransportMessageObserver(IServiceBusConfiguration configuration, IServiceBusEvents events,
            ISerializer serializer, IEnvironmentService environmentService, IProcessService processService)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(events, nameof(events));
            Guard.AgainstNull(serializer, nameof(serializer));
            Guard.AgainstNull(environmentService, nameof(environmentService));
            Guard.AgainstNull(processService, nameof(processService));

            _configuration = configuration;
            _events = events;
            _serializer = serializer;
            _environmentService = environmentService;
            _processService = processService;
            _log = Log.For(this);
        }

        public void Execute(OnDeserializeTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var receivedMessage = state.GetReceivedMessage();
            var workQueue = state.GetWorkQueue();
            var errorQueue = state.GetErrorQueue();

            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNull(workQueue, nameof(workQueue));
            Guard.AgainstNull(errorQueue, nameof(errorQueue));

            TransportMessage transportMessage;

            try
            {
                using (var stream = receivedMessage.Stream.Copy())
                {
                    transportMessage =
                        (TransportMessage) _serializer.Deserialize(typeof(TransportMessage), stream);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                _log.Error(string.Format(Resources.TransportMessageDeserializationException, workQueue.Uri.Secured(),
                    ex));

                if (_configuration.RemoveCorruptMessages)
                {
                    state.GetWorkQueue().Acknowledge(state.GetReceivedMessage().AcknowledgementToken);
                }
                else
                {
                    if (!_environmentService.UserInteractive)
                    {
                        _processService.GetCurrentProcess().Kill();
                    }

                    return;
                }

                _events.OnTransportMessageDeserializationException(this,
                    new DeserializationExceptionEventArgs(
                        pipelineEvent,
                        workQueue,
                        errorQueue,
                        ex));

                pipelineEvent.Pipeline.Abort();

                return;
            }

            state.SetTransportMessage(transportMessage);
            state.SetMessageBytes(transportMessage.Message);

            transportMessage.AcceptInvariants();
        }
    }
}