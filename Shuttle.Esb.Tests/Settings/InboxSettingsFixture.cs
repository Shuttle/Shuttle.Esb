using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class InboxSettingsFixture : SettingsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var settings = GetSettings();

            Assert.IsNotNull(settings);

            Assert.AreEqual("queue://./inbox-work", settings.Inbox.WorkQueueUri);
            Assert.AreEqual("queue://./inbox-error", settings.Inbox.ErrorQueueUri);

            Assert.AreEqual(25, settings.Inbox.ThreadCount);
            Assert.AreEqual(25, settings.Inbox.MaximumFailureCount);

            Assert.AreEqual(TimeSpan.FromMilliseconds(250), settings.Inbox.DurationToSleepWhenIdle[0]);
            Assert.AreEqual(TimeSpan.FromSeconds(10), settings.Inbox.DurationToSleepWhenIdle[1]);
            Assert.AreEqual(TimeSpan.FromSeconds(30), settings.Inbox.DurationToSleepWhenIdle[2]);

            Assert.AreEqual(TimeSpan.FromMinutes(30), settings.Inbox.DurationToIgnoreOnFailure[0]);
            Assert.AreEqual(TimeSpan.FromHours(1), settings.Inbox.DurationToIgnoreOnFailure[1]);
        }
    }
}