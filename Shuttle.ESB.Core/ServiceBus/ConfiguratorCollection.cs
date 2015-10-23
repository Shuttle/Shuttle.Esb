using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class ConfiguratorCollection : IConfigurator
	{
		private readonly List<IConfigurator> _configurators = new List<IConfigurator>();

		public void Add(IConfigurator configurator)
		{
			Guard.AgainstNull(configurator, "configurator");

			if (Contains(configurator))
			{
				throw new ESBConfigurationException(string.Format(ESBResources.ConfiguratorAlreadyRegisteredException, configurator.GetType().FullName));
			}

			_configurators.Add(configurator);
		}

		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			foreach (var configurator in _configurators)
			{
				configurator.Apply(configuration);
			}
		}

		public bool Contains(IConfigurator configurator)
		{
			Guard.AgainstNull(configurator, "configurator");

			return _configurators.Any(candidate => candidate.GetType().FullName.Equals(configurator.GetType().FullName,StringComparison.OrdinalIgnoreCase));
		}
	}
}