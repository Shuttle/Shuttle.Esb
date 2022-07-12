using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ServiceBusOptionsFixture : OptionsFixture
    {
        [Test]
        public void Should_be_able_to_load_shared_configuration()
        {
            var options = GetOptions();

            Assert.IsNotNull(options);

            Assert.IsTrue(options.RemoveMessagesNotHandled);
            Assert.IsTrue(options.RemoveCorruptMessages);
            Assert.AreEqual("GZip", options.CompressionAlgorithm);
            Assert.AreEqual("3DES", options.EncryptionAlgorithm);
        }
    }
}