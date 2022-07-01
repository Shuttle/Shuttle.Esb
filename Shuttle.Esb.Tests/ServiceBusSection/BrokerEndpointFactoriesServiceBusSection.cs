using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class BrokerEndpointFactoriesServiceBusSection : ServiceBusSectionFixture
    {
        [Test]
        [TestCase("BrokerEndpointFactories-EmptyTypes.config")]
        public void Should_be_able_to_handle_empty_types(string file)
        {
            var section = GetServiceBusSection(file);

            Assert.IsNotNull(section);
            Assert.IsNotNull(section.BrokerEndpointEndpointFactories);
            Assert.IsTrue(section.BrokerEndpointEndpointFactories.Scan);
        }

        [Test]
        [TestCase("Empty.config")]
        public void Should_be_able_to_handle_missing_element(string file)
        {
            var section = GetServiceBusSection(file);

            Assert.IsNotNull(section);
            Assert.IsNotNull(section.BrokerEndpointEndpointFactories);
            Assert.IsTrue(section.BrokerEndpointEndpointFactories.Scan);
        }

        [Test]
        [TestCase("BrokerEndpointFactories.config")]
        [TestCase("BrokerEndpointFactories-Grouped.config")]
        public void Should_be_able_to_load_the_configuration(string file)
        {
            var section = GetServiceBusSection(file);

            Assert.IsNotNull(section);
            Assert.IsNotNull(section.BrokerEndpointEndpointFactories);
            Assert.IsFalse(section.BrokerEndpointEndpointFactories.Scan);
            Assert.AreEqual(2, section.BrokerEndpointEndpointFactories.Count);

            foreach (BrokerEndpointFactoryElement queueFactoryElement in section.BrokerEndpointEndpointFactories)
            {
                Console.WriteLine(queueFactoryElement.Type);
            }
        }
    }
}