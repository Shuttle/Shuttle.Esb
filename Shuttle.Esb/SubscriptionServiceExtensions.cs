using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class SubscriptionServiceExtensions
    {
        public static IEnumerable<string> GetSubscribedUris(this ISubscriptionService subscriptionService, object message)
        {
            return Guard.AgainstNull(subscriptionService, nameof(subscriptionService)).GetSubscribedUris(Guard.AgainstNull(message, nameof(message)).GetType().FullName);
        }

        public static async Task<IEnumerable<string>> GetSubscribedUrisAsync(this ISubscriptionService subscriptionService, object message)
        {
            return await Guard.AgainstNull(subscriptionService, nameof(subscriptionService)).GetSubscribedUrisAsync(Guard.AgainstNull(message, nameof(message)).GetType().FullName).ConfigureAwait(false);
        }
    }
}