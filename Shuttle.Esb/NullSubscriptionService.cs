using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Esb;

public class NullSubscriptionService : ISubscriptionService
{
    public async Task<IEnumerable<string>> GetSubscribedUrisAsync(string messageType)
    {
        await Task.CompletedTask;

        throw new NotImplementedException("NullSubscriptionManager");
    }
}