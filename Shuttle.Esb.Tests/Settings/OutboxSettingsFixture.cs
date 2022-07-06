using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class OutboxSettingsFixture : SettingsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var settings = GetSettings();

            Assert.IsNotNull(settings);

            Assert.AreEqual("queue://./outbox-work", settings.Outbox.WorkQueueUri);
            Assert.AreEqual("queue://./outbox-error", settings.Outbox.ErrorQueueUri);

            Assert.AreEqual(25, settings.Outbox.MaximumFailureCount);

            Assert.AreEqual(TimeSpan.FromMilliseconds(250), settings.Outbox.DurationToSleepWhenIdle[0]);
            Assert.AreEqual(TimeSpan.FromSeconds(10), settings.Outbox.DurationToSleepWhenIdle[1]);
            Assert.AreEqual(TimeSpan.FromSeconds(30), settings.Outbox.DurationToSleepWhenIdle[2]);

            Assert.AreEqual(TimeSpan.FromMinutes(30), settings.Outbox.DurationToIgnoreOnFailure[0]);
            Assert.AreEqual(TimeSpan.FromHours(1), settings.Outbox.DurationToIgnoreOnFailure[1]);
        }
    }
}