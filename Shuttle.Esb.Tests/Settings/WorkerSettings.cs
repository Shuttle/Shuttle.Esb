using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class WorkerSettingsFixture : SettingsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_valid_configuration()
        {
            var settings = GetSettings();

            Assert.IsNotNull(settings);
            Assert.AreEqual("msmq://./distributor-server-control-inbox-work",
                settings.Worker.DistributorControlWorkQueueUri);
            Assert.AreEqual(5, settings.Worker.ThreadAvailableNotificationIntervalSeconds);
        }
    }
}