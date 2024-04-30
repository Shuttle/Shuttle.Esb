using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb.Tests;

[TestFixture]
public class SerializeMessageObserverFixture
{
    [Test]
    public void Should_be_able_to_serialize_message()
    {
        Should_be_able_to_serialize_message_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_serialize_message_async()
    {
        await Should_be_able_to_serialize_message_async(false);
    }

    private async Task Should_be_able_to_serialize_message_async(bool sync)
    {
        var serializer = new Mock<ISerializer>();

        var observer = new SerializeMessageObserver(serializer.Object);

        var pipeline = new Pipeline()
            .RegisterObserver(observer);

        pipeline
            .RegisterStage(".")
            .WithEvent<OnSerializeMessage>();

        var simpleCommand = new SimpleCommand();
        var transportMessage = new TransportMessage();
        var stream = new MemoryStream();

        pipeline.State.SetTransportMessage(transportMessage);
        pipeline.State.SetMessage(simpleCommand);

        serializer.Setup(m => m.Serialize(simpleCommand)).Returns(stream);
        serializer.Setup(m => m.SerializeAsync(simpleCommand)).Returns(Task.FromResult((Stream)stream));

        if (sync)
        {
            pipeline.Execute();

            serializer.Verify(m => m.Serialize(simpleCommand), Times.Once);
        }
        else
        {
            await pipeline.ExecuteAsync();

            serializer.Verify(m => m.SerializeAsync(simpleCommand), Times.Once);
        }

        Assert.That(transportMessage.Message, Is.Not.Null);
        Assert.That(pipeline.State.GetMessageBytes(), Is.SameAs(transportMessage.Message));

        serializer.VerifyNoOtherCalls();
    }
}