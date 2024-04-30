using System;
using System.Threading.Tasks;
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

        private async Task ExecuteAsync(OnDeserializeTransportMessage pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var receivedMessage = Guard.AgainstNull(state.GetReceivedMessage(), StateKeys.ReceivedMessage);
            var workQueue = Guard.AgainstNull(state.GetWorkQueue(), StateKeys.WorkQueue);

            TransportMessage transportMessage;

            try
            {
                if (sync)
                {
                    using (var stream = receivedMessage.Stream.Copy())
                    {
                        transportMessage = (TransportMessage)_serializer.Deserialize(typeof(TransportMessage), stream);
                    }
                }
                else
                {
                    using (var stream = await receivedMessage.Stream.CopyAsync().ConfigureAwait(false))
                    {
                        transportMessage = (TransportMessage)await _serializer.DeserializeAsync(typeof(TransportMessage), stream).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                TransportMessageDeserializationException?.Invoke(this, new DeserializationExceptionEventArgs(pipelineEvent, workQueue, state.GetErrorQueue(), ex));

                if (_serviceBusOptions.RemoveCorruptMessages)
                {
                    if (sync)
                    {
                        workQueue.Acknowledge(state.GetReceivedMessage().AcknowledgementToken);
                    }
                    else
                    {
                        await workQueue.AcknowledgeAsync(state.GetReceivedMessage().AcknowledgementToken).ConfigureAwait(false);
                    }
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

        public event EventHandler<DeserializationExceptionEventArgs> TransportMessageDeserializationException;

        public void Execute(OnDeserializeTransportMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnDeserializeTransportMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }
    }
}