using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class ServiceBusConfigurationExtensions
    {
        public static bool HasInbox(this IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            return serviceBusConfiguration.Inbox != null;
        }

        public static bool HasControlInbox(this IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            return serviceBusConfiguration.ControlInbox != null;
        }

        public static bool HasOutbox(this IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            return serviceBusConfiguration.Outbox != null;
        }

        public static bool IsWorker(this IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            return serviceBusConfiguration.Worker != null;
        }
    }
}