using System.Transactions;

namespace Shuttle.Esb
{
	public interface ITransactionScopeConfiguration
	{
		bool Enabled { get; }
		IsolationLevel IsolationLevel { get; }
		int TimeoutSeconds { get; }
	}
}