using Shuttle.Core.Contract;
using System;

namespace Shuttle.Esb
{
    public class MessageReleasedEventArgs : EventArgs
    {
        public object AcknowledgementToken { get; }

        public MessageReleasedEventArgs(object acknowledgementToken)
        {
            AcknowledgementToken = Guard.AgainstNull(acknowledgementToken, nameof(acknowledgementToken));
        }
    }
}