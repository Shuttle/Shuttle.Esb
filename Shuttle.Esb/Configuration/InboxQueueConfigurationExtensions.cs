using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class InboxQueueConfigurationExtensions
    {
        public static bool HasDeferredQueue(this IInboxConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            return configuration.DeferredQueue != null;
        }
    }
}