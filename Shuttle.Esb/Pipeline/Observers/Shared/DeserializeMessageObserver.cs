using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;

namespace Shuttle.Esb
{
    public interface IDeserializeMessageObserver : IPipelineObserver<OnDeserializeMessage>
    {
        event EventHandler<DeserializationExceptionEventArgs> MessageDeserializationException;
    }

    public class DeserializeMessageObserver : IDeserializeMessageObserver
    {
        private readonly ISerializer _serializer;

        public DeserializeMessageObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, nameof(serializer));

            _serializer = serializer;
        }

        public void Execute(OnDeserializeMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnDeserializeMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false);
        }

        public event EventHandler<DeserializationExceptionEventArgs> MessageDeserializationException;

        private async Task ExecuteAsync(OnDeserializeMessage pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;

            var transportMessage = Guard.AgainstNull(state.GetTransportMessage(), StateKeys.TransportMessage);

            object message;

            try
            {
                var data = transportMessage.Message;

                using (var stream = new MemoryStream(data, 0, data.Length, false, true))
                {
                    message = sync
                        ? _serializer.Deserialize(Type.GetType(transportMessage.AssemblyQualifiedName, true, true), stream)
                        : await _serializer.DeserializeAsync(Type.GetType(transportMessage.AssemblyQualifiedName, true, true), stream).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var workQueue = state.GetWorkQueue();
                var errorQueue = state.GetErrorQueue();
                var receivedMessage = state.GetReceivedMessage();

                MessageDeserializationException?.Invoke(this, new DeserializationExceptionEventArgs(pipelineEvent, workQueue, errorQueue, ex));

                if (workQueue == null || errorQueue == null || receivedMessage == null || workQueue.IsStream)
                {
                    throw;
                }

                transportMessage.RegisterFailure(ex.AllMessages(), new TimeSpan());

                if (sync)
                {
                    errorQueue.Enqueue(transportMessage, _serializer.Serialize(transportMessage));
                    workQueue.Acknowledge(receivedMessage.AcknowledgementToken);
                }
                else
                {
                    await errorQueue.EnqueueAsync(transportMessage, await _serializer.SerializeAsync(transportMessage).ConfigureAwait(false)).ConfigureAwait(false);
                    await workQueue.AcknowledgeAsync(receivedMessage.AcknowledgementToken).ConfigureAwait(false);
                }

                state.SetTransactionComplete();
                pipelineEvent.Pipeline.Abort();

                return;
            }

            state.SetMessage(message);
        }
    }
}