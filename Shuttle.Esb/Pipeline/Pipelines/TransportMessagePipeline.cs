using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class TransportMessagePipeline : Pipeline
    {
        public TransportMessagePipeline(AssembleMessageObserver assembleMessageObserver, SerializeMessageObserver serializeMessageObserver, 
            CompressMessageObserver compressMessageObserver, EncryptMessageObserver encryptMessageObserver)
        {
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

        public bool Execute(TransportMessageConfigurator configurator)
        {
            Guard.AgainstNull(configurator, "options");

            State.SetTransportMessageContext(configurator);

            return base.Execute();
        }
    }
}