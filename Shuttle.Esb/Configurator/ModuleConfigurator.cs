using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ModuleConfigurator : IConfigurator
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

				Guard.Against<EsbConfigurationException>(type == null,
					string.Format(EsbResources.UnknownTypeException, moduleElement.Type));

				types.Add(type);
			}

			foreach (var type in types)
			{
				try
				{
					type.AssertDefaultConstructor(string.Format(InfrastructureResources.DefaultConstructorRequired, "Module", type.FullName));

					configuration.Modules.Add((IPipelineModule) Activator.CreateInstance(type));
				}
				catch (Exception ex)
				{
					throw new EsbConfigurationException(string.Format(EsbResources.ModuleInstantiationException, ex.Message));
				}
			}
		}
	}
}