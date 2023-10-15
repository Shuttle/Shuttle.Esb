using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

public class FakeMessageHandlerInvoker : IMessageHandlerInvoker
{
    private readonly Dictionary<string, int> _invokeCounts = new Dictionary<string, int>();

    public MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent)
    {
        return InvokeAsync(pipelineEvent, true).GetAwaiter().GetResult();
    }

    public async Task<MessageHandlerInvokeResult> InvokeAsync(IPipelineEvent pipelineEvent)
    {
        return await InvokeAsync(pipelineEvent, false).ConfigureAwait(false);
    }

    public int GetInvokeCount(string messageType)
    {
        _invokeCounts.TryGetValue(messageType, out var count);

        return count;
    }

    private async Task<MessageHandlerInvokeResult> InvokeAsync(IPipelineEvent pipelineEvent, bool sync)
    {
        var messageType = pipelineEvent.Pipeline.State.GetTransportMessage().MessageType;

        _invokeCounts.TryGetValue(messageType, out var count);
        _invokeCounts[messageType] = count + 1;

        var messageHandlerInvokeResult = MessageHandlerInvokeResult.InvokedHandler(pipelineEvent.Pipeline.State.GetTransportMessage().AssemblyQualifiedName);

        return sync
            ? messageHandlerInvokeResult
            : await Task.FromResult(messageHandlerInvokeResult).ConfigureAwait(false);
    }
}