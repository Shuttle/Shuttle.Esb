using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ProcessorSettings
    {
        public string Uri { get; set; }
        public IBrokerEndpoint WorkBrokerEndpoint { get; set; }
        public string ErrorUri { get; set; }
        public IBrokerEndpoint ErrorBrokerEndpoint { get; set; }
        public int MaximumFailureCount { get; set; }
        public int ThreadCount { get; set; } = 1;

        public void Apply(ProcessorSettings settings)
        {
            Guard.AgainstNull(settings, nameof(settings));

            Uri = settings.Uri;
            WorkBrokerEndpoint = settings.WorkBrokerEndpoint;
            ErrorUri = settings.ErrorUri;
            ErrorBrokerEndpoint = settings.ErrorBrokerEndpoint;
            MaximumFailureCount = settings.MaximumFailureCount;
            ThreadCount = settings.ThreadCount;
        }
    }
}