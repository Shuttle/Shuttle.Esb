using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ServiceBusSettingsFixture : SettingsFixture
    {
        [Test]
        public void Should_be_able_to_load_shared_configuration()
        {
            var settings = GetSettings();

            Assert.IsNotNull(settings);

            Assert.IsTrue(settings.RemoveMessagesNotHandled);
            Assert.IsTrue(settings.RemoveCorruptMessages);
            Assert.AreEqual("GZip", settings.CompressionAlgorithm);
            Assert.AreEqual("3DES", settings.EncryptionAlgorithm);
        }
    }
}