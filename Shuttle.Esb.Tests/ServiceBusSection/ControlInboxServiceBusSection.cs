using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ControlInboxServiceBusSection : ServiceBusSectionFixture
    {
        [Test]
        [TestCase("Control-Full.config")]
        [TestCase("Control-Full-Grouped.config")]
        public void Should_be_able_to_load_a_full_configuration(string file)
        {
            var section = GetServiceBusSection(file);

            Assert.IsNotNull(section);

            Assert.AreEqual("msmq://./control-inbox-work", section.ControlInbox.WorkQueueUri);
            Assert.AreEqual("msmq://./control-inbox-error", section.ControlInbox.ErrorQueueUri);


            Assert.AreEqual(25, section.ControlInbox.ThreadCount);
            Assert.AreEqual(25, section.ControlInbox.MaximumFailureCount);

            Assert.AreEqual(TimeSpan.FromMilliseconds(250), section.ControlInbox.DurationToSleepWhenIdle[0]);
            Assert.AreEqual(TimeSpan.FromSeconds(10), section.ControlInbox.DurationToSleepWhenIdle[1]);
            Assert.AreEqual(TimeSpan.FromSeconds(30), section.ControlInbox.DurationToSleepWhenIdle[2]);

            Assert.AreEqual(TimeSpan.FromMinutes(30), section.ControlInbox.DurationToIgnoreOnFailure[0]);
            Assert.AreEqual(TimeSpan.FromHours(1), section.ControlInbox.DurationToIgnoreOnFailure[1]);
        }
    }
}