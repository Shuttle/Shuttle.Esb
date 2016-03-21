namespace Shuttle.Esb
{
	public class NullTransactionScope : ITransactionScope
	{
		public void Complete()
		{
		}

		public void Dispose()
		{
		}
	}
}