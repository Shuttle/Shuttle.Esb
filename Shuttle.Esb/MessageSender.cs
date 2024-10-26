using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class MessageSender : IMessageSender
{
    private readonly IPipelineFactory _pipelineFactory;
    private readonly ISubscriptionService _subscriptionService;

    public MessageSender(IPipelineFactory pipelineFactory, ISubscriptionService subscriptionService)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _subscriptionService = Guard.AgainstNull(subscriptionService);
    }

    public async Task DispatchAsync(TransportMessage transportMessage, TransportMessage? transportMessageReceived = null)
    {
        Guard.AgainstNull(transportMessage);

        var messagePipeline = _pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

        try
        {
            await messagePipeline.ExecuteAsync(transportMessage, transportMessageReceived).ConfigureAwait(false);
        }
        finally
        {
            _pipelineFactory.ReleasePipeline(messagePipeline);
        }
    }

    private async Task<TransportMessage> GetTransportMessageAsync(object message, TransportMessage? transportMessageReceived, Action<TransportMessageBuilder>? builder = null)
    {
        Guard.AgainstNull(message);

        var messagePipeline = _pipelineFactory.GetPipeline<TransportMessagePipeline>();

        try
        {
            await messagePipeline.ExecuteAsync(message, transportMessageReceived, builder).ConfigureAwait(false);

            return messagePipeline.State.GetTransportMessage()!;
        }
        finally
        {
            _pipelineFactory.ReleasePipeline(messagePipeline);
        }
    }

    public async Task<IEnumerable<TransportMessage>> PublishAsync(object message, TransportMessage? transportMessageReceived = null, Action<TransportMessageBuilder>? builder = null)
    {
        Guard.AgainstNull(message);

        var subscribers = (await _subscriptionService.GetSubscribedUrisAsync(message).ConfigureAwait(false)).ToList();

        if (subscribers.Count > 0)
        {
            var transportMessage = await GetTransportMessageAsync(message, transportMessageReceived, builder).ConfigureAwait(false);

            var result = new List<TransportMessage>(subscribers.Count);

            foreach (var subscriber in subscribers)
            {
                transportMessage.RecipientInboxWorkQueueUri = subscriber;

                await DispatchAsync(transportMessage, transportMessageReceived).ConfigureAwait(false);

                result.Add(transportMessage);
            }
            return result;
        }

        return Enumerable.Empty<TransportMessage>();
    }

    public async Task<TransportMessage> SendAsync(object message, TransportMessage? transportMessageReceived = null, Action<TransportMessageBuilder>? builder = null)
    {
        var transportMessage = await GetTransportMessageAsync(message, transportMessageReceived, builder).ConfigureAwait(false);

        await DispatchAsync(transportMessage, transportMessageReceived).ConfigureAwait(false);

        return transportMessage;
    }
}