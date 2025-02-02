using NUnit.Framework;
using OtherNamespace;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class RouteSpecificationFixture
{
    [Test]
    public void Should_be_able_to_use_regex()
    {
        var specification = new RegexMessageRouteSpecification("simple");

        Assert.That(specification.IsSatisfiedBy(new OtherNamespaceCommand().GetType().FullName!), Is.False);
        Assert.That(specification.IsSatisfiedBy(new SimpleCommand().GetType().FullName!), Is.True);
    }

    [Test]
    public void Should_be_able_to_use_starts_with()
    {
        var specification = new StartsWithMessageRouteSpecification("Shuttle.Esb");

        Assert.That(specification.IsSatisfiedBy(new OtherNamespaceCommand().GetType().FullName!), Is.False);
        Assert.That(specification.IsSatisfiedBy(new SimpleCommand().GetType().FullName!), Is.True);
    }

    [Test]
    public void Should_be_able_to_use_type_list()
    {
        var specification = new TypeListMessageRouteSpecification(typeof(SimpleCommand).AssemblyQualifiedName!);

        Assert.That(specification.IsSatisfiedBy(new OtherNamespaceCommand().GetType().FullName!), Is.False);
        Assert.That(specification.IsSatisfiedBy(new SimpleCommand().GetType().FullName!), Is.True);
    }
}