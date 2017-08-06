using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Tests
{
    public class FakeMessageHandlerInvoker : IMessageHandlerInvoker
    {
        private readonly Dictionary<string, int> _invokeCounts = new Dictionary<string, int>();

        public MessageHandlerInvokeResult Invoke(IPipelineEvent pipelineEvent)
        {
            var messageType = pipelineEvent.Pipeline.State.GetTransportMessage().MessageType;

            if (!_invokeCounts.ContainsKey(messageType))
            {
                _invokeCounts.Add(messageType, 0);
            }

            _invokeCounts[messageType] = _invokeCounts[messageType] + 1;

            return MessageHandlerInvokeResult.InvokedHandler(this);
        }

        public int GetInvokeCount(string messageType)
        {
            if (!_invokeCounts.ContainsKey(messageType))
            {
                return 0;
            }

            return _invokeCounts[messageType];
        }
    }
}