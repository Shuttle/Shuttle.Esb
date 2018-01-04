using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ConfiguratorCollection : IConfigurator
    {
        private readonly List<IConfigurator> _configurators = new List<IConfigurator>();

        public void Apply(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            foreach (var configurator in _configurators)
            {
                configurator.Apply(configuration);
            }
        }

        public void Add(IConfigurator configurator)
        {
            Guard.AgainstNull(configurator, nameof(configurator));

            if (Contains(configurator))
            {
                throw new EsbConfigurationException(string.Format(Resources.ConfiguratorAlreadyRegisteredException,
                    configurator.GetType().FullName));
            }

            _configurators.Add(configurator);
        }

        public bool Contains(IConfigurator configurator)
        {
            Guard.AgainstNull(configurator, nameof(configurator));

            return
                _configurators.Any(
                    candidate =>
                        candidate.GetType().FullName.Equals(configurator.GetType().FullName,
                            StringComparison.OrdinalIgnoreCase));
        }
    }
}