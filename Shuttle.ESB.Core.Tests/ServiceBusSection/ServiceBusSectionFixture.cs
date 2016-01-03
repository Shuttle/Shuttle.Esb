using System;
using System.IO;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core.Tests
{
    public class ServiceBusSectionFixture
    {
        protected ServiceBusSection GetServiceBusSection(string file)
        {
            return ConfigurationSectionProvider.OpenFile<ServiceBusSection>("shuttle", "serviceBus", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@".\ServiceBusSection\files\{0}", file)));
        }
    }
}