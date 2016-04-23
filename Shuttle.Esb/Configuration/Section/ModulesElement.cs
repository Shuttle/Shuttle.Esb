using System;
using System.Configuration;

namespace Shuttle.Esb
{
	public class ModulesElement : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new ModuleElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return Guid.NewGuid();
		}
	}
}