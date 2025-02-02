using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class MessageReleasedEventArgs : EventArgs
{
    public MessageReleasedEventArgs(object acknowledgementToken)
    {
        AcknowledgementToken = Guard.AgainstNull(acknowledgementToken);
    }

    public object AcknowledgementToken { get; }
}