namespace Shuttle.Esb
{
    public static class StateKeys
    {
        public const string AvailableWorker = "AvailableWorker";
        public const string CancellationToken = "CancellationToken";
        public const string DeferredMessageReturned = "DeferredMessageReturned";
        public const string DeferredBrokerEndpoint = "DeferredBrokerEndpoint";
        public const string DurationToIgnoreOnFailure = "DurationToIgnoreOnFailure";
        public const string ErrorBrokerEndpoint = "ErrorBrokerEndpoint";
        public const string ShouldProcess = "ShouldProcess";
        public const string HandlerContext = "HandlerContext";
        public const string MaximumFailureCount = "MaximumFailureCount";
        public const string Message = "Message";
        public const string MessageBytes = "MessageBytes";
        public const string MessageHandler = "MessageHandler";
        public const string ProcessingStatus = "ProcessingStatus";
        public const string ReceivedMessage = "ReceivedMessage";
        public const string TransportMessage = "TransportMessage";
        public const string TransportMessageConfigurator = "TransportMessageConfigurator";
        public const string TransportMessageReceived = "TransportMessageReceived";
        public const string TransportMessageStream = "TransportMessageStream";
        public const string Working = "Working";
        public const string BrokerEndpoint = "BrokerEndpoint";
    }
}