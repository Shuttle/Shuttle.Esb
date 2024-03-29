﻿using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;
using Shuttle.Core.System;

namespace Shuttle.Esb
{
    public interface IDeserializeTransportMessageObserver : IPipelineObserver<OnDeserializeTransportMessage>
    {
        event EventHandler<DeserializationExceptionEventArgs> TransportMessageDeserializationException;
    }

    public class DeserializeTransportMessageObserver : IDeserializeTransportMessageObserver
    {
        private readonly ServiceBusOptions _serviceBusOptions;
        private readonly IEnvironmentService _environmentService;
        private readonly IProcessService _processService;
        private readonly ISerializer _serializer;

        public DeserializeTransportMessageObserver(IOptions<ServiceBusOptions> serviceBusOptions,
            ISerializer serializer, IEnvironmentService environmentService, IProcessService processService)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(serializer, nameof(serializer));
            Guard.AgainstNull(environmentService, nameof(environmentService));
            Guard.AgainstNull(processService, nameof(processService));

            _serviceBusOptions = serviceBusOptions.Value;
            _serializer = serializer;
            _environmentService = environmentService;
            _processService = processService;
        }

        public void Execute(OnDeserializeTransportMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var receivedMessage = state.GetReceivedMessage();
            var workQueue = state.GetWorkQueue();

            Guard.AgainstNull(receivedMessage, nameof(receivedMessage));
            Guard.AgainstNull(workQueue, nameof(workQueue));

            TransportMessage transportMessage;

            try
            {
                using (var stream = receivedMessage.Stream.Copy())
                {
                    transportMessage =
                        (TransportMessage)_serializer.Deserialize(typeof(TransportMessage), stream);
                }
            }
            catch (Exception ex)
            {
                TransportMessageDeserializationException(this,
                    new DeserializationExceptionEventArgs(pipelineEvent, workQueue, state.GetErrorQueue(), ex));

                if (_serviceBusOptions.RemoveCorruptMessages)
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

                pipelineEvent.Pipeline.Abort();

                return;
            }

            state.SetTransportMessage(transportMessage);
            state.SetMessageBytes(transportMessage.Message);

            transportMessage.AcceptInvariants();
        }

        public event EventHandler<DeserializationExceptionEventArgs> TransportMessageDeserializationException = delegate
        {
        };
    }
}