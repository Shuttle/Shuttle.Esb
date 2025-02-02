using NUnit.Framework;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class MessageRoutesFixture
{
    [Test]
    public void Should_be_able_to_create_new_routes()
    {
        var route = new MessageRoute(new("route://"));
        var routes = new MessageRouteCollection();

        route.AddSpecification(new RegexMessageRouteSpecification("simple"));

        routes.Add(route);

        Assert.That(routes.FindAll(new SimpleCommand().GetType().FullName ?? string.Empty)[0].Uri, Is.SameAs(route.Uri));
    }
}