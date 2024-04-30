using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class IdempotenceOptionsFixture : OptionsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_full_configuration()
        {
            var options = GetOptions();

            Assert.IsNotNull(options);

            Assert.That(options.Idempotence.ConnectionStringName, Is.EqualTo("connection-string"));
        }
    }
}