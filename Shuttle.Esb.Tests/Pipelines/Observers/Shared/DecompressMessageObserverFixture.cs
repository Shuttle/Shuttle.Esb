using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Compression;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class DecompressMessageObserverFixture
{
    [Test]
    public async Task Should_be_able_to_skip_when_decompression_is_not_required_async()
    {
        var compressionService = new Mock<ICompressionService>();

        var observer = new DecompressMessageObserver(compressionService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDecompressMessage>();

        pipeline.State.SetTransportMessage(new());

        await pipeline.ExecuteAsync();

        compressionService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_decompress_message_async()
    {
        var compressionAlgorithm = new Mock<ICompressionAlgorithm>();
        var compressionService = new Mock<ICompressionService>();

        var observer = new DecompressMessageObserver(compressionService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDecompressMessage>();

        var transportMessage = new TransportMessage { CompressionAlgorithm = "gzip" };

        compressionService.Setup(m => m.Get(transportMessage.CompressionAlgorithm)).Returns(compressionAlgorithm.Object);

        pipeline.State.SetTransportMessage(transportMessage);

        await pipeline.ExecuteAsync();

        compressionAlgorithm.Verify(m => m.DecompressAsync(It.IsAny<byte[]>()), Times.Once);

        compressionService.Verify(m => m.Get(transportMessage.CompressionAlgorithm), Times.Once);

        compressionService.VerifyNoOtherCalls();
        compressionAlgorithm.VerifyNoOtherCalls();
    }
}