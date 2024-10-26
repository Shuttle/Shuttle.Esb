using System;
using System.Linq;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class MessageRouteProviderFixture
{
    [Test]
    public void Should_be_able_to_add_or_merge_message_routes()
    {
        var nullQueueUri = new Uri("null://queue/");
        const string firstMessageType = "first-message-type";
        const string secondMessageType = "second-message-type";

        var provider = new MessageRouteProvider(Options.Create(new ServiceBusOptions()));

        Assert.That(provider.GetRouteUris(firstMessageType).Any(), Is.False);

        provider.Add(new MessageRoute(nullQueueUri).AddSpecification(new StartsWithMessageRouteSpecification("first")));

        Assert.That(provider.GetRouteUris(firstMessageType).Any(), Is.True);
        Assert.That(provider.GetRouteUris(secondMessageType).Any(), Is.False);
        Assert.That(provider.GetRouteUris(firstMessageType).First(), Is.EqualTo(nullQueueUri));

        provider.Add(new MessageRoute(nullQueueUri).AddSpecification(new StartsWithMessageRouteSpecification("second")));

        Assert.That(provider.GetRouteUris(firstMessageType).Any(), Is.True);
        Assert.That(provider.GetRouteUris(secondMessageType).Any(), Is.True);
        Assert.That(provider.GetRouteUris(firstMessageType).First(), Is.EqualTo(nullQueueUri));
        Assert.That(provider.GetRouteUris(secondMessageType).First(), Is.EqualTo(nullQueueUri));
    }
}