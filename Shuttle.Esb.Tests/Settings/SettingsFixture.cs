using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Shuttle.Esb.Tests
{
    public class SettingsFixture
    {
        protected ServiceBusOptions GetSettings()
        {
            var result = new ServiceBusOptions();

            result.Inbox.DurationToSleepWhenIdle.Clear();
            result.Inbox.DurationToIgnoreOnFailure.Clear();
            result.Outbox.DurationToSleepWhenIdle.Clear();
            result.Outbox.DurationToIgnoreOnFailure.Clear();
            result.ControlInbox.DurationToSleepWhenIdle.Clear();
            result.ControlInbox.DurationToIgnoreOnFailure.Clear();

            new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\Settings\appsettings.json")).Build()
                .GetSection(ServiceBusOptions.SectionName).Bind(result);

            return result;
        }
    }
}