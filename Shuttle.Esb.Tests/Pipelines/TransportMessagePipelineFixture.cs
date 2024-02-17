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
    public void Should_be_able_execute_transport_message_pipeline_with_optimal_performance()
    {
        Should_be_able_execute_transport_message_pipeline_with_optimal_performance_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_execute_transport_message_pipeline_with_optimal_performance_async()
    {
        await Should_be_able_execute_transport_message_pipeline_with_optimal_performance_async(false).ConfigureAwait(false);
    }

    private async Task Should_be_able_execute_transport_message_pipeline_with_optimal_performance_async(bool sync)
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

            pipeline.State.Replace(StateKeys.Message, new object());

            if (sync)
            {
                pipeline.Execute();
            }
            else
            {
                await pipeline.ExecuteAsync().ConfigureAwait(false);
            }

            pipelineFactory.ReleasePipeline(pipeline);

            count++;
        }

        sw.Stop();

        Console.WriteLine($@"[transport-message-assembly] : count = {count} / ms = {sw.ElapsedMilliseconds}");

        Assert.That(count, Is.GreaterThan(1000));
    }
}