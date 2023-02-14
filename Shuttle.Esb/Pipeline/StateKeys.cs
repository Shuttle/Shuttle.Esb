namespace Shuttle.Esb
{
    public static class StateKeys
    {
        public const string AvailableWorker = "AvailableWorker";
        public const string DeferredMessageReturned = "DeferredMessageReturned";
        public const string DeferredQueue = "DeferredQueue";
        public const string DurationToIgnoreOnFailure = "DurationToIgnoreOnFailure";
        public const string ErrorQueue = "ErrorQueue";
        public const string ShouldProcess = "ShouldProcess";
        public const string HandlerContext = "HandlerContext";
        public const string MaximumFailureCount = "MaximumFailureCount";
        public const string Message = "Message";
        public const string MessageBytes = "MessageBytes";
        public const string MessageHandler = "MessageHandler";
        public const string ProcessingStatus = "ProcessingStatus";
        public const string ReceivedMessage = "ReceivedMessage";
        public const string TransportMessage = "TransportMessage";
        public const string TransportMessageBuilder = "TransportMessageBuilder";
        public const string TransportMessageReceived = "TransportMessageReceived";
        public const string TransportMessageStream = "TransportMessageStream";
        public const string Working = "Working";
        public const string WorkQueue = "WorkQueue";
    }
}