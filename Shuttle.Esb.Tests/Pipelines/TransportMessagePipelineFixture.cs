using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class TransportMessagePipelineFixture
{
    [Test]
    public async Task Should_be_able_execute_transport_message_pipeline_with_optimal_performance_async()
    {
        var services = new ServiceCollection();

        services.AddServiceBus();

        var serviceProvider = services.BuildServiceProvider();

        var pipelineFactory = serviceProvider.GetRequiredService<IPipelineFactory>();

        var sw = new Stopwatch();

        sw.Start();

        var count = 0;

        while (sw.ElapsedMilliseconds < 1000)
        {
            var pipeline = pipelineFactory.GetPipeline<TransportMessagePipeline>();

            pipeline.State.Replace(StateKeys.Message, new());

            await pipeline.ExecuteAsync().ConfigureAwait(false);

            pipelineFactory.ReleasePipeline(pipeline);

            count++;
        }

        sw.Stop();

        Console.WriteLine($@"[transport-message-assembly] : count = {count} / ms = {sw.ElapsedMilliseconds}");

        Assert.That(count, Is.GreaterThan(1000));
    }
}