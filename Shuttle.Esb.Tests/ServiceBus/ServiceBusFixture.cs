using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class ServiceBusFixture
{
    [Test]
    public async Task Should_be_able_to_handle_expired_message_async()
    {
        var handlerInvoker = new FakeMessageHandlerInvoker();
        var fakeQueue = new FakeQueue(2);

        var queueService = new Mock<IQueueService>();

        queueService.Setup(m => m.Get(It.IsAny<string>())).Returns(fakeQueue);

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddTransactionScope(builder =>
        {
            builder.Options.Enabled = false;
        });
        services.AddSingleton(queueService.Object);
        services.AddSingleton<IMessageHandlerInvoker>(handlerInvoker);
        services.AddServiceBus(builder =>
        {
            builder.Options.Inbox = new()
            {
                WorkQueueUri = "fake://work",
                ErrorQueueUri = "fake://error",
                ThreadCount = 1
            };
        });

        var serviceBus = services.BuildServiceProvider().GetRequiredService<IServiceBus>();

        await using (await serviceBus.StartAsync())
        {
            var timeout = DateTime.Now.AddSeconds(5);

            while (fakeQueue.MessageCount < 2 && DateTime.Now < timeout)
            {
                Thread.Sleep(5);
            }
        }

        Assert.That(handlerInvoker.GetInvokeCount("SimpleCommand"), Is.EqualTo(1), "FakeHandlerInvoker was not invoked exactly once.");
        Assert.That(fakeQueue.MessageCount, Is.EqualTo(2), "FakeQueue was not invoked exactly twice.");
    }
}