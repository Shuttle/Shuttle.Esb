using System;

namespace Shuttle.Esb
{
    public class HandlerMessageMethodMissingException : Exception
    {
        public HandlerMessageMethodMissingException(string message) : base(message)
        {
        }
    }
}