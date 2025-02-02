using System;

namespace Shuttle.Esb;

public class SendMessageException : Exception
{
    public SendMessageException(string message) : base(message)
    {
    }
}