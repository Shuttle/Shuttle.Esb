using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class MessageAcknowledgedEventArgs : EventArgs
    {
        public object AcknowledgementToken { get; }

        public MessageAcknowledgedEventArgs(object acknowledgementToken)
        {
            AcknowledgementToken = Guard.AgainstNull(acknowledgementToken, nameof(acknowledgementToken));
        }
    }
}