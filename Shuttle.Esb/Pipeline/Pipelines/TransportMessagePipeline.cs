using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class TransportMessagePipeline : Pipeline
    {
        public TransportMessagePipeline(IAssembleMessageObserver assembleMessageObserver, ISerializeMessageObserver serializeMessageObserver, ICompressMessageObserver compressMessageObserver, IEncryptMessageObserver encryptMessageObserver)
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

        public bool Execute(TransportMessageBuilder builder)
        {
            Guard.AgainstNull(builder, nameof(builder));

            State.SetTransportMessageContext(builder);

            return base.Execute();
        }
    }
}