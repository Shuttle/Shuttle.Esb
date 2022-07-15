namespace Shuttle.Esb
{
    public class InboxOptions : ProcessorOptions
    {
        public InboxOptions()
        {
            ThreadCount = 5;
        }

        public string DeferredQueueUri { get; set; }
        public bool Distribute { get; set; } = false;
        public int DistributeSendCount { get; set; } = 5;
    }
}