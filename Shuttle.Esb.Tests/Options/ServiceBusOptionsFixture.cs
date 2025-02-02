using NUnit.Framework;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class ServiceBusOptionsFixture : OptionsFixture
{
    [Test]
    public void Should_be_able_to_load_shared_configuration()
    {
        var options = GetOptions();

        Assert.That(options, Is.Not.Null);

        Assert.That(options.RemoveMessagesNotHandled, Is.True);
        Assert.That(options.RemoveCorruptMessages, Is.True);
        Assert.That(options.CompressionAlgorithm, Is.EqualTo("GZip"));
        Assert.That(options.EncryptionAlgorithm, Is.EqualTo("3DES"));
    }
}