using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class OutboxOptionsFixture : OptionsFixture
{
    [Test]
    public void Should_be_able_to_load_a_full_configuration()
    {
        var options = GetOptions();

        Assert.That(options, Is.Not.Null);

        Assert.That(options.Outbox!.WorkQueueUri, Is.EqualTo("queue://./outbox-work"));
        Assert.That(options.Outbox.ErrorQueueUri, Is.EqualTo("queue://./outbox-error"));

        Assert.That(options.Outbox.MaximumFailureCount, Is.EqualTo(25));

        Assert.That(options.Outbox.DurationToSleepWhenIdle[0], Is.EqualTo(TimeSpan.FromMilliseconds(250)));
        Assert.That(options.Outbox.DurationToSleepWhenIdle[1], Is.EqualTo(TimeSpan.FromSeconds(10)));
        Assert.That(options.Outbox.DurationToSleepWhenIdle[2], Is.EqualTo(TimeSpan.FromSeconds(30)));

        Assert.That(options.Outbox.DurationToIgnoreOnFailure[0], Is.EqualTo(TimeSpan.FromMinutes(30)));
        Assert.That(options.Outbox.DurationToIgnoreOnFailure[1], Is.EqualTo(TimeSpan.FromHours(1)));
    }
}