using System;
using System.IO;

namespace Shuttle.ESB.Core.Tests
{
    public class ServiceBusSectionFixture
    {
        protected ServiceBusSection GetServiceBusSection(string file)
        {
            return ShuttleConfigurationSection.Open<ServiceBusSection>("serviceBus", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@".\ServiceBusSection\files\{0}", file)));
        }
    }
}