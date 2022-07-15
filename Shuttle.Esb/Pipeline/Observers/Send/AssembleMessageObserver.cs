using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public interface IAssembleMessageObserver : IPipelineObserver<OnAssembleMessage>
    {
    }

    public class AssembleMessageObserver : IAssembleMessageObserver
    {
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private readonly IIdentityProvider _identityProvider;
        private readonly ServiceBusOptions _serviceBusOptions;

        public AssembleMessageObserver(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IIdentityProvider identityProvider)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));
            Guard.AgainstNull(identityProvider, nameof(identityProvider));

            _serviceBusOptions = serviceBusOptions.Value;
            _serviceBusConfiguration = serviceBusConfiguration;
            _identityProvider = identityProvider;
        }

        public void Execute(OnAssembleMessage pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessageConfigurator =
                state.Get<TransportMessageBuilder>(StateKeys.TransportMessageBuilder);

            Guard.AgainstNull(transportMessageConfigurator, nameof(transportMessageConfigurator));
            Guard.AgainstNull(transportMessageConfigurator.Message, "transportMessageConfigurator.Message");

            state.SetTransportMessage(transportMessageConfigurator.TransportMessage(_serviceBusOptions, _serviceBusConfiguration, _identityProvider));
            state.SetMessage(transportMessageConfigurator.Message);
        }
    }
}