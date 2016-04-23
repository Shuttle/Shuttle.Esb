namespace Shuttle.Esb
{
	public interface IRequireInitialization
	{
		void Initialize(IServiceBus bus);
	}
}