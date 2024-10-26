using NUnit.Framework;
using OtherNamespace;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class MessageRouteFixture
{
    [Test]
    public void Should_be_able_to_create_a_new_route()
    {
        var map = new MessageRoute(new("route://"));

        map.AddSpecification(new RegexMessageRouteSpecification("simple"));

        Assert.That(map.IsSatisfiedBy(new OtherNamespaceCommand().GetType().FullName ?? string.Empty), Is.False);
        Assert.That(map.IsSatisfiedBy(new SimpleCommand().GetType().FullName ?? string.Empty), Is.True);
    }
}