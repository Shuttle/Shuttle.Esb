using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class OutboxOptionsFixture : OptionsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var options = GetOptions();

            Assert.IsNotNull(options);

            Assert.AreEqual("queue://./outbox-work", options.Outbox.WorkQueueUri);
            Assert.AreEqual("queue://./outbox-error", options.Outbox.ErrorQueueUri);

            Assert.AreEqual(25, options.Outbox.MaximumFailureCount);

            Assert.AreEqual(TimeSpan.FromMilliseconds(250), options.Outbox.DurationToSleepWhenIdle[0]);
            Assert.AreEqual(TimeSpan.FromSeconds(10), options.Outbox.DurationToSleepWhenIdle[1]);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.Outbox.DurationToSleepWhenIdle[2]);

            Assert.AreEqual(TimeSpan.FromMinutes(30), options.Outbox.DurationToIgnoreOnFailure[0]);
            Assert.AreEqual(TimeSpan.FromHours(1), options.Outbox.DurationToIgnoreOnFailure[1]);
        }
    }
}