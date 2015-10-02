namespace Shuttle.ESB.Core
{
	public interface ITransactionScopeFactory
	{
		ITransactionScope Create();
	}
}