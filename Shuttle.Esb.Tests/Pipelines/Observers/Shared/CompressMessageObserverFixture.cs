using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Compression;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class CompressMessageObserverFixture
{
    [Test]
    public void Should_be_able_to_skip_when_compression_is_not_required()
    {
        Should_be_able_to_skip_when_compression_is_not_required_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_skip_when_compression_is_not_required_async()
    {
        await Should_be_able_to_skip_when_compression_is_not_required_async(false);
    }

    private async Task Should_be_able_to_skip_when_compression_is_not_required_async(bool sync)
    {
        var compressionService = new Mock<ICompressionService>();

        var observer = new CompressMessageObserver(compressionService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnCompressMessage>();

        pipeline.State.SetTransportMessage(new TransportMessage());

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        compressionService.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_compress_message()
    {
        Should_be_able_to_compress_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_compress_message_async()
    {
        await Should_be_able_to_compress_message_async(false);
    }

    private async Task Should_be_able_to_compress_message_async(bool sync)
    {
        var compressionAlgorithm = new Mock<ICompressionAlgorithm>();
        var compressionService = new Mock<ICompressionService>();

        var observer = new CompressMessageObserver(compressionService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnCompressMessage>();

        var transportMessage = new TransportMessage { CompressionAlgorithm = "gzip" };

        compressionService.Setup(m => m.Get(transportMessage.CompressionAlgorithm)).Returns(compressionAlgorithm.Object);

        pipeline.State.SetTransportMessage(transportMessage);

        if (sync)
        {
            pipeline.Execute();

            compressionAlgorithm.Verify(m => m.Compress(It.IsAny<byte[]>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            compressionAlgorithm.Verify(m => m.CompressAsync(It.IsAny<byte[]>()), Times.Once);
        }

        compressionService.Verify(m => m.Get(transportMessage.CompressionAlgorithm), Times.Once);

        compressionService.VerifyNoOtherCalls();
        compressionAlgorithm.VerifyNoOtherCalls();
    }
}