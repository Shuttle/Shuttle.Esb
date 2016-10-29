using System;
using System.Collections;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ModuleCollection : IEnumerable<IPipelineModule>, IDisposable
	{
		private readonly List<IPipelineModule> _modules = new List<IPipelineModule>();

		public IEnumerator<IPipelineModule> GetEnumerator()
		{
			return _modules.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(IPipelineModule module)
		{
			Guard.AgainstNull(module, "module");

			_modules.Add(module);
		}

		public void Dispose()
		{
			foreach (var module in _modules)
			{
				module.AttemptDispose();
			}
		}
	}
}