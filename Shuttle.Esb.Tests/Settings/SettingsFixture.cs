using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Shuttle.Esb.Tests
{
    public class SettingsFixture
    {
        protected ServiceBusSettings GetSettings()
        {
            var result = new ServiceBusSettings();

            new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\Settings\appsettings.json")).Build()
                .GetSection(ServiceBusSettings.SectionName).Bind(result);

            return result;
        }
    }
}