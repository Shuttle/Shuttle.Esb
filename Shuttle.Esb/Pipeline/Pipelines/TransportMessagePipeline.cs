using System;
using System.Threading;
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
            RegisterStage("Create")
                .WithEvent<OnAssembleMessage>()
                .WithEvent<OnAfterAssembleMessage>()
                .WithEvent<OnSerializeMessage>()
                .WithEvent<OnAfterSerializeMessage>()
                .WithEvent<OnEncryptMessage>()
                .WithEvent<OnAfterEncryptMessage>()
                .WithEvent<OnCompressMessage>()
                .WithEvent<OnAfterCompressMessage>();

            RegisterObserver(Guard.AgainstNull(assembleMessageObserver, nameof(assembleMessageObserver)));
            RegisterObserver(Guard.AgainstNull(serializeMessageObserver, nameof(serializeMessageObserver)));
            RegisterObserver(Guard.AgainstNull(compressMessageObserver, nameof(compressMessageObserver)));
            RegisterObserver(Guard.AgainstNull(encryptMessageObserver, nameof(encryptMessageObserver)));
        }

        public bool Execute(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder, CancellationToken cancellationToken = default)
        {
            return ExecuteAsync(message, transportMessageReceived, builder, cancellationToken, true).GetAwaiter().GetResult();
        }

        public async Task<bool> ExecuteAsync(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(message, transportMessageReceived, builder, cancellationToken, false).ConfigureAwait(false);
        }

        private async Task<bool> ExecuteAsync(object message, TransportMessage transportMessageReceived, Action<TransportMessageBuilder> builder, CancellationToken cancellationToken, bool sync)
        {
            State.SetMessage(Guard.AgainstNull(message, nameof(message)));
            State.SetTransportMessageReceived(transportMessageReceived);
            State.SetTransportMessageBuilder(builder);

            if (sync)
            {
                return base.Execute(cancellationToken);
            }
            else
            {
                return await base.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}