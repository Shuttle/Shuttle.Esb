using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class TransportMessageFixture
{
    [Test]
    public void Should_be_able_to_determine_if_message_has_an_expiry_date()
    {
        Assert.That(new TransportMessage().HasExpiryDate(), Is.False);

        Assert.That(new TransportMessage { ExpiryDate = DateTime.MaxValue }.HasExpiryDate(), Is.False);

        Assert.That(new TransportMessage { ExpiryDate = DateTime.Now.AddSeconds(30) }.HasExpiryDate(), Is.True);
    }

    [Test]
    public void Should_be_able_to_determine_if_message_has_expired()
    {
        Assert.That(new TransportMessage().HasExpired(), Is.False);

        Assert.That(new TransportMessage { ExpiryDate = DateTime.MaxValue }.HasExpired(), Is.False);

        Assert.That(new TransportMessage { ExpiryDate = DateTime.Now.AddSeconds(-30) }.HasExpired(), Is.True);
    }

    [Test]
    public void Should_be_able_to_determine_if_message_should_be_ignored()
    {
        var transportMessage = new TransportMessage
        {
            IgnoreTillDate = DateTime.Now.AddMinutes(1)
        };

        Assert.That(transportMessage.IsIgnoring(), Is.True);

        transportMessage.IgnoreTillDate = DateTime.Now.AddMilliseconds(-1);

        Assert.That(transportMessage.IsIgnoring(), Is.False);
    }

    [Test]
    public void Should_be_able_to_register_failures_and_have_IgnoreTillDate_set()
    {
        var message = new TransportMessage();

        var before = DateTime.UtcNow;

        message.RegisterFailure("failure");

        Assert.That(before <= message.IgnoreTillDate, Is.True);

        message = new();

        var durationToIgnoreOnFailure =
            new[]
            {
                TimeSpan.FromMinutes(3),
                TimeSpan.FromMinutes(30),
                TimeSpan.FromHours(2)
            };

        Assert.That(DateTime.UtcNow.AddMinutes(3) <= message.IgnoreTillDate, Is.False);

        message.RegisterFailure("failure", durationToIgnoreOnFailure[0]);

        var ignoreTillDate = DateTime.UtcNow.AddMinutes(3);

        Assert.That(ignoreTillDate.AddMilliseconds(-100) < message.IgnoreTillDate && ignoreTillDate.AddMilliseconds(100) > message.IgnoreTillDate, Is.True);
        Assert.That(DateTime.UtcNow.AddMinutes(30) < message.IgnoreTillDate, Is.False);

        message.RegisterFailure("failure", durationToIgnoreOnFailure[1]);

        ignoreTillDate = DateTime.UtcNow.AddMinutes(30);

        Assert.That(ignoreTillDate.AddMilliseconds(-100) < message.IgnoreTillDate && ignoreTillDate.AddMilliseconds(100) > message.IgnoreTillDate, Is.True);
        Assert.That(DateTime.UtcNow.AddHours(2) < message.IgnoreTillDate, Is.False);

        message.RegisterFailure("failure", durationToIgnoreOnFailure[2]);

        ignoreTillDate = DateTime.UtcNow.AddHours(2);

        Assert.That(ignoreTillDate.AddMilliseconds(-100) < message.IgnoreTillDate && ignoreTillDate.AddMilliseconds(100) > message.IgnoreTillDate, Is.True);
    }
}