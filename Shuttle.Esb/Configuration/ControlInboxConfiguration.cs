using System;

namespace Shuttle.Esb
{
    public class ControlInboxConfiguration : IControlInboxConfiguration
    {
        public IQueue WorkQueue { get; set; }
        public IQueue ErrorQueue { get; set; }
    }
}