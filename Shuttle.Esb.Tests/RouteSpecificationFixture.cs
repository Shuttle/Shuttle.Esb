using NUnit.Framework;
using Shuttle.Esb.Tests.Messages;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class RouteSpecificationFixture
    {
        [Test]
        public void Should_be_able_to_use_regex()
        {
            var specification = new RegexMessageRouteSpecification("simple");

            Assert.IsFalse(specification.IsSatisfiedBy(new OtherNamespaceCommand().GetType().FullName));
            Assert.IsTrue(specification.IsSatisfiedBy(new SimpleCommand().GetType().FullName));
        }

        [Test]
        public void Should_be_able_to_use_starts_with()
        {
            var specification = new StartsWithMessageRouteSpecification("Shuttle.Esb");

            Assert.IsFalse(specification.IsSatisfiedBy(new OtherNamespaceCommand().GetType().FullName));
            Assert.IsTrue(specification.IsSatisfiedBy(new SimpleCommand().GetType().FullName));
        }

        [Test]
        public void Should_be_able_to_use_typelist()
        {
            var specification = new TypeListMessageRouteSpecification(typeof(SimpleCommand).AssemblyQualifiedName);

            Assert.IsFalse(specification.IsSatisfiedBy(new OtherNamespaceCommand().GetType().FullName));
            Assert.IsTrue(specification.IsSatisfiedBy(new SimpleCommand().GetType().FullName));
        }
    }
}