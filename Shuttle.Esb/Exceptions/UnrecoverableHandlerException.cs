using System;

namespace Shuttle.Esb;

public class UnrecoverableHandlerException : Exception
{
    public UnrecoverableHandlerException()
    {
    }

    public UnrecoverableHandlerException(string message) : base(message)
    {
    }

    public UnrecoverableHandlerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}