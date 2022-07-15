using System;

namespace Shuttle.Esb
{
    public class ControlInboxOptions : ProcessorOptions
    {
        public ControlInboxOptions()
        {
            ThreadCount = 1;
        }
    }
}