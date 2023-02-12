using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class TransportMessagePipeline : Pipeline
    {
        public TransportMessagePipeline(IAssembleMessageObserver assembleMessageObserver,
            ISerializeMessageObserver serializeMessageObserver, ICompressMessageObserver compressMessageObserver,
            IEncryptMessageObserver encryptMessageObserver)
        {
            Guard.AgainstNull(assembleMessageObserver, nameof(assembleMessageObserver));
            Guard.AgainstNull(serializeMessageObserver, nameof(serializeMessageObserver));
            Guard.AgainstNull(compressMessageObserver, nameof(compressMessageObserver));
            Guard.AgainstNull(encryptMessageObserver, nameof(encryptMessageObserver));

            RegisterStage("Create")
                .WithEvent<OnAssembleMessage>()
                .WithEvent<OnAfterAssembleMessage>()
                .WithEvent<OnSerializeMessage>()
                .WithEvent<OnAfterSerializeMessage>()
                .WithEvent<OnEncryptMessage>()
                .WithEvent<OnAfterEncryptMessage>()
                .WithEvent<OnCompressMessage>()
                .WithEvent<OnAfterCompressMessage>();

            RegisterObserver(assembleMessageObserver);
            RegisterObserver(serializeMessageObserver);
            RegisterObserver(compressMessageObserver);
            RegisterObserver(encryptMessageObserver);
        }

        public async Task<bool> Execute(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder)
        {
            Guard.AgainstNull(message, nameof(message));

            State.SetMessage(message);
            State.SetTransportMessageReceived(transportMessageReceived);
            State.SetTransportMessageBuilder(builder);

            return await base.Execute().ConfigureAwait(false);
        }
    }
}