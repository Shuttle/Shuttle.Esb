using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class DispatchTransportMessagePipelineFixture
{
    [Test]
    public async Task Should_be_able_to_execute_dispatch_transport_message_pipeline_with_optimal_performance_async()
    {
        var recipientInboxWorkQueueUri = new Uri("queue://null/null");

        var queueService = new Mock<IQueueService>();

        queueService.Setup(m => m.Get(recipientInboxWorkQueueUri.ToString())).Returns(new Mock<IQueue>().Object);

        var services = new ServiceCollection();

        services.AddSingleton(queueService.Object);

        services.AddServiceBus();

        var serviceProvider = services.BuildServiceProvider();

        var pipelineFactory = serviceProvider.GetRequiredService<IPipelineFactory>();

        var transportMessage = new TransportMessage
        {
            Message = Array.Empty<byte>(),
            RecipientInboxWorkQueueUri = recipientInboxWorkQueueUri.ToString()
        };

        var sw = new Stopwatch();

        sw.Start();

        var count = 0;

        while (sw.ElapsedMilliseconds < 1000)
        {
            var pipeline = pipelineFactory.GetPipeline<DispatchTransportMessagePipeline>();

            pipeline.State.Replace(StateKeys.TransportMessage, transportMessage);

            await pipeline.ExecuteAsync().ConfigureAwait(false);

            pipelineFactory.ReleasePipeline(pipeline);

            count++;
        }

        sw.Stop();

        Console.WriteLine($@"[message-dispatch] : count = {count} / ms = {sw.ElapsedMilliseconds}");

        Assert.That(count, Is.GreaterThan(1000));
    }
}