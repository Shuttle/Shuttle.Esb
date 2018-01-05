using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class TranportMessageFixture
    {
        [Test]
        public void Should_be_able_to_determine_if_message_has_an_expiry_date()
        {
            Assert.IsFalse(new TransportMessage().HasExpiryDate());

            Assert.IsFalse(new TransportMessage
            {
                ExpiryDate = DateTime.MaxValue
            }.HasExpiryDate());

            Assert.IsTrue(new TransportMessage
            {
                ExpiryDate = DateTime.Now.AddSeconds(30)
            }.HasExpiryDate());
        }

        [Test]
        public void Should_be_able_to_determine_if_message_has_expired()
        {
            Assert.IsFalse(new TransportMessage().HasExpired());

            Assert.IsFalse(new TransportMessage
            {
                ExpiryDate = DateTime.MaxValue
            }.HasExpired());

            Assert.IsTrue(new TransportMessage
            {
                ExpiryDate = DateTime.Now.AddSeconds(-30)
            }.HasExpired());
        }

        [Test]
        public void Should_be_able_to_determine_if_message_should_be_ignored()
        {
            var messsage = new TransportMessage
            {
                IgnoreTillDate = DateTime.Now.AddMinutes(1)
            };

            Assert.IsTrue(messsage.IsIgnoring());

            messsage.IgnoreTillDate = DateTime.Now.AddMilliseconds(-1);

            Assert.IsFalse(messsage.IsIgnoring());
        }

        [Test]
        public void Should_be_able_to_register_failures_and_have_IgnoreTillDate_set()
        {
            var message = new TransportMessage();

            var before = DateTime.Now;

            message.RegisterFailure("failure");

            Assert.IsTrue(before <= message.IgnoreTillDate);

            message = new TransportMessage();

            var durationToIgnoreOnFailure =
                new[]
                {
                    TimeSpan.FromMinutes(3),
                    TimeSpan.FromMinutes(30),
                    TimeSpan.FromHours(2)
                };

            Assert.IsFalse(DateTime.Now.AddMinutes(3) <= message.IgnoreTillDate);

            message.RegisterFailure("failure", durationToIgnoreOnFailure[0]);

            Assert.IsTrue(DateTime.Now.AddMinutes(3) <= message.IgnoreTillDate);
            Assert.IsFalse(DateTime.Now.AddMinutes(30) <= message.IgnoreTillDate);

            message.RegisterFailure("failure", durationToIgnoreOnFailure[1]);

            Assert.IsTrue(DateTime.Now.AddMinutes(30) <= message.IgnoreTillDate);
            Assert.IsFalse(DateTime.Now.AddHours(2) <= message.IgnoreTillDate);

            message.RegisterFailure("failure", durationToIgnoreOnFailure[2]);

            Assert.IsTrue(DateTime.Now.AddHours(2) <= message.IgnoreTillDate);
        }
    }
}