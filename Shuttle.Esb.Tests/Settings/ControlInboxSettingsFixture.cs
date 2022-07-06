using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ControlInboxSettingsFixture : SettingsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var settings = GetSettings();

            Assert.IsNotNull(settings);

            Assert.AreEqual("queue://./control-inbox-work", settings.ControlInbox.WorkQueueUri);
            Assert.AreEqual("queue://./control-inbox-error", settings.ControlInbox.ErrorQueueUri);


            Assert.AreEqual(25, settings.ControlInbox.ThreadCount);
            Assert.AreEqual(25, settings.ControlInbox.MaximumFailureCount);

            Assert.AreEqual(TimeSpan.FromMilliseconds(250), settings.ControlInbox.DurationToSleepWhenIdle[0]);
            Assert.AreEqual(TimeSpan.FromSeconds(10), settings.ControlInbox.DurationToSleepWhenIdle[1]);
            Assert.AreEqual(TimeSpan.FromSeconds(30), settings.ControlInbox.DurationToSleepWhenIdle[2]);

            Assert.AreEqual(TimeSpan.FromMinutes(30), settings.ControlInbox.DurationToIgnoreOnFailure[0]);
            Assert.AreEqual(TimeSpan.FromHours(1), settings.ControlInbox.DurationToIgnoreOnFailure[1]);
        }
    }
}