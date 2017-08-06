using System;

namespace Shuttle.Esb
{
    public class ProcessorException : Exception
    {
        public ProcessorException(string message) : base(message)
        {
        }
    }
}