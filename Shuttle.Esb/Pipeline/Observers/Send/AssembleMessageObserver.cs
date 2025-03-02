using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public interface IAssembleMessageObserver : IPipelineObserver<OnAssembleMessage>
{
}

public class AssembleMessageObserver : IAssembleMessageObserver
{
    private readonly IIdentityProvider _identityProvider;

    private readonly IServiceBusConfiguration _serviceBusConfiguration;
    private readonly ServiceBusOptions _serviceBusOptions;

    public AssembleMessageObserver(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IIdentityProvider identityProvider)
    {
        _serviceBusOptions = Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value);
        _serviceBusConfiguration = Guard.AgainstNull(serviceBusConfiguration);
        _identityProvider = Guard.AgainstNull(identityProvider);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAssembleMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var builder = state.GetTransportMessageBuilder();
        var message = Guard.AgainstNull(state.GetMessage());
        var transportMessageReceived = state.GetTransportMessageReceived();

        var identity = _identityProvider.Get();

        var transportMessage = new TransportMessage
        {
            SenderInboxWorkQueueUri = _serviceBusConfiguration.HasInbox()
                ? Guard.AgainstNull(_serviceBusConfiguration.Inbox!.WorkQueue).Uri.ToString()
                : string.Empty,
            PrincipalIdentityName = Guard.AgainstNull(Guard.AgainstNull(identity).Name),
            MessageType = Guard.AgainstNullOrEmptyString(message.GetType().FullName),
            AssemblyQualifiedName = Guard.AgainstNullOrEmptyString(message.GetType().AssemblyQualifiedName),
            EncryptionAlgorithm = _serviceBusOptions.EncryptionAlgorithm,
            CompressionAlgorithm = _serviceBusOptions.CompressionAlgorithm,
            SendDate = DateTimeOffset.UtcNow
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

            transportMessage.RecipientInboxWorkQueueUri = Guard.AgainstNull(_serviceBusConfiguration.Inbox!.WorkQueue).Uri.ToString();
        }

        if (transportMessageBuilder.ShouldReply)
        {
            if (transportMessageReceived == null || string.IsNullOrEmpty(transportMessageReceived.SenderInboxWorkQueueUri))
            {
                throw new InvalidOperationException(Resources.SendReplyException);
            }

            transportMessage.RecipientInboxWorkQueueUri = transportMessageReceived.SenderInboxWorkQueueUri;
        }

        if (transportMessage.IgnoreTillDate > DateTimeOffset.UtcNow &&
            _serviceBusConfiguration.HasInbox() &&
            Guard.AgainstNull(_serviceBusConfiguration.Inbox!.WorkQueue).IsStream)
        {
            throw new InvalidOperationException(Resources.DeferStreamException);
        }

        state.SetTransportMessage(transportMessage);

        await Task.CompletedTask;
    }
}