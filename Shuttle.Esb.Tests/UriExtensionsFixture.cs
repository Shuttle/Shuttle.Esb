using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class UriExtensionsFixture
    {
        [Test]
        public void Should_be_able_to_secure_a_uri()
        {
            Assert.AreEqual("uri://the-host/path",
                new Uri("uri://username:password@the-host/path").Secured().ToString());
        }
    }
}