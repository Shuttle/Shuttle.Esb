using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public static class SubscriptionServiceExtensions
{
    public static async Task<IEnumerable<string>> GetSubscribedUrisAsync(this ISubscriptionService subscriptionService, object message)
    {
        return await Guard.AgainstNull(subscriptionService).GetSubscribedUrisAsync(Guard.AgainstNullOrEmptyString(Guard.AgainstNull(message).GetType().FullName)).ConfigureAwait(false);
    }
}