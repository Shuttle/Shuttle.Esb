using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public static class InboxQueueConfigurationExtensions
{
    public static bool HasDeferredQueue(this IInboxConfiguration configuration)
    {
        return Guard.AgainstNull(configuration).DeferredQueue != null;
    }
}