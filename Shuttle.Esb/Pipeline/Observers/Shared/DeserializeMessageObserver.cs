using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransactionScope;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb;

public interface IDeserializeMessageObserver : IPipelineObserver<OnDeserializeMessage>
{
    event EventHandler<DeserializationExceptionEventArgs>? MessageDeserializationException;
}

public class DeserializeMessageObserver : IDeserializeMessageObserver
{
    private readonly ISerializer _serializer;

    public DeserializeMessageObserver(ISerializer serializer)
    {
        _serializer = Guard.AgainstNull(serializer);
    }

    public event EventHandler<DeserializationExceptionEventArgs>? MessageDeserializationException;

    public async Task ExecuteAsync(IPipelineContext<OnDeserializeMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        var transportMessage = Guard.AgainstNull(state.GetTransportMessage());

        object message;

        try
        {
            var data = transportMessage.Message;

            using (var stream = new MemoryStream(data, 0, data.Length, false, true))
            {
                message = await _serializer.DeserializeAsync(Guard.AgainstNull(Type.GetType(Guard.AgainstNull(transportMessage.AssemblyQualifiedName), true, true)), stream).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            var workQueue = Guard.AgainstNull(state.GetWorkQueue());
            var errorQueue = Guard.AgainstNull(state.GetErrorQueue());
            var receivedMessage = state.GetReceivedMessage();

            MessageDeserializationException?.Invoke(this, new(pipelineContext, workQueue, errorQueue, ex));

            if (workQueue == null || errorQueue == null || receivedMessage == null || workQueue.IsStream)
            {
                throw;
            }

            transportMessage.RegisterFailure(ex.AllMessages(), new TimeSpan());

            await errorQueue.EnqueueAsync(transportMessage, await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false)).ConfigureAwait(false);
            await workQueue.AcknowledgeAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);

            state.SetTransactionScopeCompleted();
            pipelineContext.Pipeline.Abort();

            return;
        }

        state.SetMessage(message);
    }
}