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

            Assert.AreEqual("msmq://./control-inbox-work", section.Control.Uri);
            Assert.AreEqual("msmq://./control-inbox-error", section.Control.ErrorUri);


            Assert.AreEqual(25, section.Control.ThreadCount);
            Assert.AreEqual(25, section.Control.MaximumFailureCount);

            Assert.AreEqual(TimeSpan.FromMilliseconds(250), section.Control.DurationToSleepWhenIdle[0]);
            Assert.AreEqual(TimeSpan.FromSeconds(10), section.Control.DurationToSleepWhenIdle[1]);
            Assert.AreEqual(TimeSpan.FromSeconds(30), section.Control.DurationToSleepWhenIdle[2]);

            Assert.AreEqual(TimeSpan.FromMinutes(30), section.Control.DurationToIgnoreOnFailure[0]);
            Assert.AreEqual(TimeSpan.FromHours(1), section.Control.DurationToIgnoreOnFailure[1]);
        }
    }
}