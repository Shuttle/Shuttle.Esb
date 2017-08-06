using System;

namespace Shuttle.Esb
{
    public class EsbConfigurationException : Exception
    {
        public EsbConfigurationException(string message) : base(message)
        {
        }
    }
}