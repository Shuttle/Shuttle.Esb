using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class StartupModulesObserver : IStartupModulesObserver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceBusConfiguration _serviceBusConfiguration;

        public StartupModulesObserver(IServiceProvider serviceProvider, IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceProvider, nameof(serviceProvider));
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            _serviceProvider = serviceProvider;
            _serviceBusConfiguration = serviceBusConfiguration;
        }

        public void Execute(OnStarted pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            foreach (var type in _serviceBusConfiguration.Modules)
            {
                _serviceProvider.GetService(type);
            }
        }
    }

    public interface IStartupModulesObserver : IPipelineObserver<OnStarted>
    {
    }
}