using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

public class FakeMessageHandlerInvoker : IMessageHandlerInvoker
{
    private readonly Dictionary<string, int> _invokeCounts = new();

    public int GetInvokeCount(string messageType)
    {
        _invokeCounts.TryGetValue(messageType, out var count);

        return count;
    }

    public async ValueTask<bool> InvokeAsync(IPipelineContext<OnHandleMessage> pipelineContext)
    {
        var transportMessage = Guard.AgainstNull(pipelineContext.Pipeline.State.GetTransportMessage());
        var messageType = transportMessage.MessageType;

        _invokeCounts.TryGetValue(messageType, out var count);
        _invokeCounts[messageType] = count + 1;

        return await ValueTask.FromResult(true).ConfigureAwait(false);
    }
}