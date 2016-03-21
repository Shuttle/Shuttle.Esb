using System;

namespace Shuttle.Esb
{
    public class ESBConfigurationException : Exception
    {
        public ESBConfigurationException(string message) : base(message)
        {
        }
    }
}