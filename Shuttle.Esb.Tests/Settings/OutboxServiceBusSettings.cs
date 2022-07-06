using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class OutboxServiceBusSettings : ServiceBusSettingsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var section = GetSettings();

            Assert.IsNotNull(section);

            Assert.AreEqual("queue://./outbox-work", section.Outbox.WorkQueueUri);
            Assert.AreEqual("queue://./outbox-error", section.Outbox.ErrorQueueUri);

            Assert.AreEqual(25, section.Outbox.MaximumFailureCount);

            Assert.AreEqual(TimeSpan.FromMilliseconds(250), section.Outbox.DurationToSleepWhenIdle[0]);
            Assert.AreEqual(TimeSpan.FromSeconds(10), section.Outbox.DurationToSleepWhenIdle[1]);
            Assert.AreEqual(TimeSpan.FromSeconds(30), section.Outbox.DurationToSleepWhenIdle[2]);

            Assert.AreEqual(TimeSpan.FromMinutes(30), section.Outbox.DurationToIgnoreOnFailure[0]);
            Assert.AreEqual(TimeSpan.FromHours(1), section.Outbox.DurationToIgnoreOnFailure[1]);
        }
    }
}