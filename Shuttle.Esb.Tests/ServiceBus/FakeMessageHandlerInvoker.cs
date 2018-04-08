using System.Collections.Generic;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests
{
    public class FakeMessageHandlerInvoker : IMessageHandlerInvoker
    {
        private readonly Dictionary<string, int> _invokeCounts = new Dictionary<string, int>();

        public MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent)
        {
            var messageType = pipelineEvent.Pipeline.State.GetTransportMessage().MessageType;

            _invokeCounts.TryGetValue(messageType, out int count);
            _invokeCounts[messageType] = count + 1;

            return MessageHandlerInvokeResult.InvokedHandler(this);
        }

        public int GetInvokeCount(string messageType)
        {
            _invokeCounts.TryGetValue(messageType, out int count);
            return count;
        }
    }
}