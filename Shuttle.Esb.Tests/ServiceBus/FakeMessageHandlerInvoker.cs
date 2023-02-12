using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests
{
    public class FakeMessageHandlerInvoker : IMessageHandlerInvoker
    {
        private readonly Dictionary<string, int> _invokeCounts = new Dictionary<string, int>();

        public async Task<MessageHandlerInvokeResult> Invoke(IPipelineEvent pipelineEvent)
        {
            var messageType = pipelineEvent.Pipeline.State.GetTransportMessage().MessageType;

            _invokeCounts.TryGetValue(messageType, out int count);
            _invokeCounts[messageType] = count + 1;

            return await Task.FromResult(MessageHandlerInvokeResult.InvokedHandler(this)).ConfigureAwait(false);
        }

        public int GetInvokeCount(string messageType)
        {
            _invokeCounts.TryGetValue(messageType, out int count);
            return count;
        }
    }
}