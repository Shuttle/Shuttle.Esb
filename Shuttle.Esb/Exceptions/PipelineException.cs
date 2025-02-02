using System;

namespace Shuttle.Esb;

public class PipelineException : Exception
{
    public PipelineException(string message) : base(message)
    {
    }

    public PipelineException(string message, Exception innerException) : base(message, innerException)
    {
    }
}