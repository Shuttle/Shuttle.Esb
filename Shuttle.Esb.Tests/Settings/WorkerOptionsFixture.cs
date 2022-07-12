using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class WorkerOptionsFixture : OptionsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_valid_configuration()
        {
            var options = GetOptions();

            Assert.IsNotNull(options);
            Assert.AreEqual("msmq://./distributor-server-control-inbox-work",
                options.Worker.DistributorControlWorkQueueUri);
            Assert.AreEqual(5, options.Worker.ThreadAvailableNotificationIntervalSeconds);
        }
    }
}