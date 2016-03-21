using System;

namespace Shuttle.Esb
{
	public interface ITransactionScope : IDisposable
	{
		void Complete();
	}
}