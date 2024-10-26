using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class MessageAcknowledgedEventArgs : EventArgs
{
    public MessageAcknowledgedEventArgs(object acknowledgementToken)
    {
        AcknowledgementToken = Guard.AgainstNull(acknowledgementToken);
    }

    public object AcknowledgementToken { get; }
}