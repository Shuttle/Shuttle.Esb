using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ControlInboxOptionsFixture : OptionsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var options = GetOptions();

            Assert.IsNotNull(options);

            Assert.AreEqual("queue://./control-inbox-work", options.ControlInbox.WorkQueueUri);
            Assert.AreEqual("queue://./control-inbox-error", options.ControlInbox.ErrorQueueUri);


            Assert.AreEqual(25, options.ControlInbox.ThreadCount);
            Assert.AreEqual(25, options.ControlInbox.MaximumFailureCount);

            Assert.AreEqual(TimeSpan.FromMilliseconds(250), options.ControlInbox.DurationToSleepWhenIdle[0]);
            Assert.AreEqual(TimeSpan.FromSeconds(10), options.ControlInbox.DurationToSleepWhenIdle[1]);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.ControlInbox.DurationToSleepWhenIdle[2]);

            Assert.AreEqual(TimeSpan.FromMinutes(30), options.ControlInbox.DurationToIgnoreOnFailure[0]);
            Assert.AreEqual(TimeSpan.FromHours(1), options.ControlInbox.DurationToIgnoreOnFailure[1]);
        }
    }
}