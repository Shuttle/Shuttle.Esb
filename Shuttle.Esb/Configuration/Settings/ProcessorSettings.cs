using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ProcessorSettings
    {
        public string WorkQueueUri { get; set; }
        public IQueue WorkQueue { get; set; }
        public string ErrorQueueUri { get; set; }
        public IQueue ErrorQueue { get; set; }
        public int MaximumFailureCount { get; set; }
        public int ThreadCount { get; set; } = 1;

        public void Apply(ProcessorSettings settings)
        {
            Guard.AgainstNull(settings, nameof(settings));

            WorkQueueUri = settings.WorkQueueUri;
            WorkQueue = settings.WorkQueue;
            ErrorQueueUri = settings.ErrorQueueUri;
            ErrorQueue = settings.ErrorQueue;
            MaximumFailureCount = settings.MaximumFailureCount;
            ThreadCount = settings.ThreadCount;
        }
    }
}