using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class ServiceBusExtensions
    {
        public static void Stop(this IServiceBus serviceBus)
        {
            Guard.AgainstNull(serviceBus, nameof(serviceBus)).StopAsync().GetAwaiter().GetResult();
        }
    }
}