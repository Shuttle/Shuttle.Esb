using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IAssembleMessageObserver : IPipelineObserver<OnAssembleMessage>
    {
    }

    public class AssembleMessageObserver : IAssembleMessageObserver
    {
        private static readonly string AnonymousName = new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous").Name;

        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IIdentityProvider _identityProvider;
        private readonly ServiceBusOptions _serviceBusOptions;

        public AssembleMessageObserver(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IIdentityProvider identityProvider)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(identityProvider, nameof(identityProvider));

            _serviceBusOptions = serviceBusOptions.Value;
            _serviceBusConfiguration = serviceBusConfiguration;
            _identityProvider = identityProvider;
        }

        public void Execute(OnAssembleMessage pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            var state = pipelineEvent.Pipeline.State;
            var builder = state.GetTransportMessageBuilder();
            var message = state.GetMessage();
            var transportMessageReceived = state.GetTransportMessageReceived();

            Guard.AgainstNull(message, nameof(message));

            var identity = _identityProvider.Get();

            var transportMessage = new TransportMessage
            {
                SenderInboxWorkQueueUri = _serviceBusConfiguration.HasInbox()
                    ? _serviceBusConfiguration.Inbox.WorkQueue.Uri.ToString()
                    : string.Empty,
                PrincipalIdentityName = identity != null
                    ? identity.Name
                    : AnonymousName,
                MessageType = message.GetType().FullName,
                AssemblyQualifiedName = message.GetType().AssemblyQualifiedName,
                EncryptionAlgorithm = _serviceBusOptions.EncryptionAlgorithm,
                CompressionAlgorithm = _serviceBusOptions.CompressionAlgorithm,
                SendDate = DateTime.UtcNow
            };

            if (transportMessageReceived != null)
            {
                transportMessage.MessageReceivedId = transportMessageReceived.MessageId;
                transportMessage.CorrelationId = transportMessageReceived.CorrelationId;
                transportMessage.Headers.AddRange(transportMessageReceived.Headers);
            }

            var transportMessageBuilder = new TransportMessageBuilder(transportMessage);

            builder?.Invoke(transportMessageBuilder);

            if (transportMessageBuilder.ShouldSendLocal)
            {
                if (!_serviceBusConfiguration.HasInbox())
                {
                    throw new InvalidOperationException(Resources.SendToSelfException);
                }

                transportMessage.RecipientInboxWorkQueueUri = _serviceBusConfiguration.Inbox.WorkQueue.Uri.ToString();
            }

            if (transportMessageBuilder.ShouldReply)
            {
                if (transportMessageReceived == null || string.IsNullOrEmpty(transportMessageReceived.SenderInboxWorkQueueUri))
                {
                    throw new InvalidOperationException(Resources.SendReplyException);
                }

                transportMessage.RecipientInboxWorkQueueUri = transportMessageReceived.SenderInboxWorkQueueUri;
            }

            if (transportMessage.IgnoreTillDate > DateTime.UtcNow &&
                _serviceBusConfiguration.HasInbox() &&
                _serviceBusConfiguration.Inbox.WorkQueue.IsStream)
            {
                throw new InvalidOperationException(Resources.DeferStreamException);
            }

            state.SetTransportMessage(transportMessage);
        }

        public async Task ExecuteAsync(OnAssembleMessage pipelineEvent)
        {
            Execute(pipelineEvent);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}