using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public class NullSubscriptionService : ISubscriptionService
    {
        public IEnumerable<string> GetSubscribedUris(string messageType)
        {
            throw new NotImplementedException("NullSubscriptionManager");
        }

        public async Task<IEnumerable<string>> GetSubscribedUrisAsync(string messageType)
        {
            await Task.CompletedTask;

            throw new NotImplementedException("NullSubscriptionManager");
        }
    }
}