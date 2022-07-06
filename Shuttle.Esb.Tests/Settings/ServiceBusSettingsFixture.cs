using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Shuttle.Esb.Tests
{
    public class ServiceBusSettingsFixture
    {
        protected Esb.ServiceBusSettings GetSettings()
        {
            var result = new Esb.ServiceBusSettings();

            new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\Settings\appsettings.json")).Build()
                .GetSection(Esb.ServiceBusSettings.SectionName).Bind(result);

            return result;
        }
    }
}