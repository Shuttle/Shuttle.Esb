using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class EncryptMessageObserverFixture
{
    [Test]
    public async Task Should_be_able_to_skip_when_encryption_is_not_required_async()
    {
        var encryptionService = new Mock<IEncryptionService>();

        var observer = new EncryptMessageObserver(encryptionService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnEncryptMessage>();

        pipeline.State.SetTransportMessage(new());

        await pipeline.ExecuteAsync();

        encryptionService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_encrypt_message_async()
    {
        var encryptionAlgorithm = new Mock<IEncryptionAlgorithm>();
        var encryptionService = new Mock<IEncryptionService>();

        var observer = new EncryptMessageObserver(encryptionService.Object);

        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object)
            .AddObserver(observer);

        pipeline
            .AddStage(".")
            .WithEvent<OnEncryptMessage>();

        var transportMessage = new TransportMessage { EncryptionAlgorithm = "3des" };

        encryptionService.Setup(m => m.Get(transportMessage.EncryptionAlgorithm)).Returns(encryptionAlgorithm.Object);

        pipeline.State.SetTransportMessage(transportMessage);

        await pipeline.ExecuteAsync();

        encryptionAlgorithm.Verify(m => m.EncryptAsync(It.IsAny<byte[]>()), Times.Once);

        encryptionService.Verify(m => m.Get(transportMessage.EncryptionAlgorithm), Times.Once);

        encryptionService.VerifyNoOtherCalls();
        encryptionAlgorithm.VerifyNoOtherCalls();
    }
}