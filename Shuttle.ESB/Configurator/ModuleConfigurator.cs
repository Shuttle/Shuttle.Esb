using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ModuleConfigurator: IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			if (ServiceBusConfiguration.ServiceBusSection == null || ServiceBusConfiguration.ServiceBusSection.Modules == null)
			{
				return;
			}

			var types = new List<Type>();

			foreach (ModuleElement moduleElement in ServiceBusConfiguration.ServiceBusSection.Modules)
			{
				var type = Type.GetType(moduleElement.Type);

				Guard.Against<ESBConfigurationException>(type == null,
					string.Format(ESBResources.UnknownTypeException, moduleElement.Type));

				types.Add(type);
			}

			foreach (var type in types)
			{
				try
				{
					type.AssertDefaultConstructor(string.Format(ESBResources.DefaultConstructorRequired, "Module", type.FullName));

					configuration.Modules.Add((IModule)Activator.CreateInstance(type));
				}
				catch (Exception ex)
				{
					throw new ESBConfigurationException(string.Format(ESBResources.ModuleInstantiationException, ex.Message));
				}
			}
		}
	}
}