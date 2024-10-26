using System;

namespace Shuttle.Esb;

public class MessageHandlerInvokerException : Exception
{
    public MessageHandlerInvokerException(string message) : base(message)
    {
    }
}