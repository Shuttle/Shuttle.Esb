namespace Shuttle.Esb
{
	public interface ITransactionScopeFactory
	{
		ITransactionScope Create();
	}
}