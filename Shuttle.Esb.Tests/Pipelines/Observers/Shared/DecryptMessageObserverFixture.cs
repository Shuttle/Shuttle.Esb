using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class DecryptMessageObserverFixture
{
    [Test]
    public void Should_be_able_to_skip_when_decryption_is_not_required()
    {
        Should_be_able_to_skip_when_decryption_is_not_required_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_skip_when_decryption_is_not_required_async()
    {
        await Should_be_able_to_skip_when_decryption_is_not_required_async(false);
    }

    private async Task Should_be_able_to_skip_when_decryption_is_not_required_async(bool sync)
    {
        var encryptionService = new Mock<IEncryptionService>();

        var observer = new DecryptMessageObserver(encryptionService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDecryptMessage>();

        pipeline.State.SetTransportMessage(new TransportMessage());

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync();
        }

        encryptionService.VerifyNoOtherCalls();
    }

    [Test]
    public void Should_be_able_to_decrypt_message()
    {
        Should_be_able_to_decrypt_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_decrypt_message_async()
    {
        await Should_be_able_to_decrypt_message_async(false);
    }

    private async Task Should_be_able_to_decrypt_message_async(bool sync)
    {
        var encryptionAlgorithm = new Mock<IEncryptionAlgorithm>();
        var encryptionService = new Mock<IEncryptionService>();

        var observer = new DecryptMessageObserver(encryptionService.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnDecryptMessage>();

        var transportMessage = new TransportMessage { EncryptionAlgorithm = "3des" };

        encryptionService.Setup(m => m.Get(transportMessage.EncryptionAlgorithm)).Returns(encryptionAlgorithm.Object);

        pipeline.State.SetTransportMessage(transportMessage);

        if (sync)
        {
            pipeline.Execute();

            encryptionAlgorithm.Verify(m => m.Decrypt(It.IsAny<byte[]>()), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            encryptionAlgorithm.Verify(m => m.DecryptAsync(It.IsAny<byte[]>()), Times.Once);
        }

        encryptionService.Verify(m => m.Get(transportMessage.EncryptionAlgorithm), Times.Once);

        encryptionService.VerifyNoOtherCalls();
        encryptionAlgorithm.VerifyNoOtherCalls();
    }
}