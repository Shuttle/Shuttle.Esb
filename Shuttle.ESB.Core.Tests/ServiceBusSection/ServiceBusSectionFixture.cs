namespace Shuttle.ESB.Core.Tests
{
    public class ServiceBusSectionFixture
    {
        protected ServiceBusSection GetServiceBusSection(string file)
        {
			return ShuttleConfigurationSection.Open<ServiceBusSection>("serviceBus", string.Format(@".\ServiceBusSection\files\{0}", file));
        }
    }
}