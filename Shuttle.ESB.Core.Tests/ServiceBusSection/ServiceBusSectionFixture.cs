using Shuttle.ESB.Core;

namespace Shuttle.ESB.Test.Integration
{
    public class ServiceBusSectionFixture
    {
        protected ServiceBusSection GetServiceBusSection(string file)
        {
			return ShuttleConfigurationSection.Open<ServiceBusSection>("serviceBus", string.Format(@".\ConfigurationSections\ServiceBusSection\files\{0}", file));
        }

		protected T GetServiceBusSection<T>(string file) where T : class
        {
			return ShuttleConfigurationSection.Open<T>("serviceBus", string.Format(@".\ConfigurationSections\ServiceBusSection\files\{0}", file));
        }
    }
}