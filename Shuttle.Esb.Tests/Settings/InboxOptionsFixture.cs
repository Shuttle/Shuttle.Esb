using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class InboxOptionsFixture : OptionsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var options = GetOptions();

            Assert.IsNotNull(options);

            Assert.AreEqual("queue://./inbox-work", options.Inbox.WorkQueueUri);
            Assert.AreEqual("queue://./inbox-error", options.Inbox.ErrorQueueUri);

            Assert.AreEqual(25, options.Inbox.ThreadCount);
            Assert.AreEqual(25, options.Inbox.MaximumFailureCount);

            Assert.AreEqual(TimeSpan.FromMilliseconds(250), options.Inbox.DurationToSleepWhenIdle[0]);
            Assert.AreEqual(TimeSpan.FromSeconds(10), options.Inbox.DurationToSleepWhenIdle[1]);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.Inbox.DurationToSleepWhenIdle[2]);

            Assert.AreEqual(TimeSpan.FromMinutes(30), options.Inbox.DurationToIgnoreOnFailure[0]);
            Assert.AreEqual(TimeSpan.FromHours(1), options.Inbox.DurationToIgnoreOnFailure[1]);
        }
    }
}