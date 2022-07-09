using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class InboxQueueConfigurationExtensions
    {
        public static bool HasDeferredQueue(this IInboxQueueConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            return configuration.DeferredQueue != null;
        }
    }
}