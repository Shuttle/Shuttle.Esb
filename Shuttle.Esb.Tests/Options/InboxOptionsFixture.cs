using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class InboxOptionsFixture : OptionsFixture
{
    [Test]
    public void Should_be_able_to_load_a_full_configuration()
    {
        var options = GetOptions();

        Assert.That(options, Is.Not.Null);

        Assert.That(options.Inbox!.WorkQueueUri, Is.EqualTo("queue://./inbox-work"));
        Assert.That(options.Inbox.ErrorQueueUri, Is.EqualTo("queue://./inbox-error"));

        Assert.That(options.Inbox.ThreadCount, Is.EqualTo(25));
        Assert.That(options.Inbox.MaximumFailureCount, Is.EqualTo(25));

        Assert.That(options.Inbox.DurationToSleepWhenIdle[0], Is.EqualTo(TimeSpan.FromMilliseconds(250)));
        Assert.That(options.Inbox.DurationToSleepWhenIdle[1], Is.EqualTo(TimeSpan.FromSeconds(10)));
        Assert.That(options.Inbox.DurationToSleepWhenIdle[2], Is.EqualTo(TimeSpan.FromSeconds(30)));

        Assert.That(options.Inbox.DurationToIgnoreOnFailure[0], Is.EqualTo(TimeSpan.FromMinutes(30)));
        Assert.That(options.Inbox.DurationToIgnoreOnFailure[1], Is.EqualTo(TimeSpan.FromHours(1)));

        Assert.That(options.Inbox.DeferredMessageProcessorResetInterval, Is.EqualTo(TimeSpan.FromMinutes(5)));
    }
}