using System;
using System.Collections;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ModuleCollection : IEnumerable<IModule>, IDisposable
	{
		private readonly List<IModule> _modules = new List<IModule>();

		public IEnumerator<IModule> GetEnumerator()
		{
			return _modules.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(IModule module)
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