using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Shuttle.Esb.Tests;

public class OptionsFixture
{
    protected ServiceBusOptions GetOptions()
    {
        var result = new ServiceBusOptions();

        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\Options\appsettings.json")).Build()
            .GetSection(ServiceBusOptions.SectionName).Bind(result);

        return result;
    }
}