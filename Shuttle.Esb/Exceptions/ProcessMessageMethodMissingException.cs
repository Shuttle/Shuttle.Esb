using System;

namespace Shuttle.Esb
{
    public class ProcessMessageMethodMissingException : Exception
    {
        public ProcessMessageMethodMissingException(string message) : base(message)
        {
        }
    }
}