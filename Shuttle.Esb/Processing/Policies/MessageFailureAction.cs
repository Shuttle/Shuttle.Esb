using System;

namespace Shuttle.Esb
{
    public class MessageFailureAction
    {
        public MessageFailureAction(bool retry, TimeSpan timeSpanToIgnoreRetriedMessage)
        {
            Retry = retry;
            TimeSpanToIgnoreRetriedMessage = timeSpanToIgnoreRetriedMessage;
        }

        public bool Retry { get; }
        public TimeSpan TimeSpanToIgnoreRetriedMessage { get; }
    }
}