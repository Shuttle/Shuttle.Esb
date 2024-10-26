using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class ServiceBusPerformanceFixture
{
    [Test]
    public async Task Should_be_able_to_send_messages_with_optimal_performance_async()
    {
        var services = new ServiceCollection();

        var messageRouteProvider = new Mock<IMessageRouteProvider>();

        messageRouteProvider.Setup(m => m.GetRouteUris(It.IsAny<string>())).Returns(new List<string> { "null-queue://null/null" });
        messageRouteProvider.Setup(m => m.GetRouteUrisAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<string>>(new List<string> { "null-queue://null/null" }));

        services.AddSingleton(messageRouteProvider.Object);
        services.AddSingleton<IQueueFactory, NullQueueFactory>();

        services.AddServiceBus();

        var serviceProvider = services.BuildServiceProvider();
        var serviceBus = serviceProvider.GetRequiredService<IServiceBus>();

        var count = 0;

        await using (await serviceBus.StartAsync())
        {
            var sw = new Stopwatch();

            sw.Start();

            while (sw.ElapsedMilliseconds < 1000)
            {
                await serviceBus.SendAsync(new SimpleCommand($"{Guid.NewGuid()}"));

                count++;
            }

            sw.Stop();

            Console.WriteLine($@"[service-bus-send] : count = {count} / ms = {sw.ElapsedMilliseconds}");

            Assert.That(count, Is.GreaterThan(1000));
        }

        Assert.That(count, Is.GreaterThan(1000));
    }
}