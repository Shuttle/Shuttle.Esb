using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ServiceBusSettings : ServiceBusSettingsFixture
    {
        [Test]
        public void Should_be_able_to_load_shared_configuration()
        {
            var section = GetSettings();

            Assert.IsNotNull(section);

            Assert.IsTrue(section.RemoveMessagesNotHandled);
            Assert.IsTrue(section.RemoveCorruptMessages);
            Assert.AreEqual("GZip", section.CompressionAlgorithm);
            Assert.AreEqual("3DES", section.EncryptionAlgorithm);
        }
    }
}