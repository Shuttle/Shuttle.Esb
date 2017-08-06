using System;

namespace Shuttle.Esb
{
    public class WorkerException : Exception
    {
        public WorkerException(string message) : base(message)
        {
        }
    }
}