using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class TransportMessagePipeline : Pipeline
    {
        public TransportMessagePipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterStage("Create")
                .WithEvent<OnAssembleMessage>()
                .WithEvent<OnAfterAssembleMessage>()
                .WithEvent<OnSerializeMessage>()
                .WithEvent<OnAfterSerializeMessage>()
                .WithEvent<OnEncryptMessage>()
                .WithEvent<OnAfterEncryptMessage>()
                .WithEvent<OnCompressMessage>()
                .WithEvent<OnAfterCompressMessage>();

            RegisterObserver(list.Get<IAssembleMessageObserver>());
            RegisterObserver(list.Get<ISerializeMessageObserver>());
            RegisterObserver(list.Get<ICompressMessageObserver>());
            RegisterObserver(list.Get<IEncryptMessageObserver>());
        }

        public bool Execute(TransportMessageConfigurator configurator)
        {
            Guard.AgainstNull(configurator, nameof(configurator));

            State.SetTransportMessageContext(configurator);

            return base.Execute();
        }
    }
}