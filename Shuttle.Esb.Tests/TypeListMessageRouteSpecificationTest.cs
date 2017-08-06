using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class TypeListMessageRouteSpecificationTest
    {
        [Test]
        public void Should_be_able_to_get_types_from_given_valid_value_string()
        {
            new TypeListMessageRouteSpecification(
                "Shuttle.Esb.Tests.SimpleCommand, Shuttle.Esb.Tests;" +
                "Shuttle.Esb.Tests.SimpleEvent, Shuttle.Esb.Tests");
        }

        [Test]
        public void Should_fail_when_given_a_type_that_cannot_be_determined()
        {
            Assert.Throws<MessageRouteSpecificationException>(() => new TypeListMessageRouteSpecification("bogus"));
        }
    }
}