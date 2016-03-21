namespace Shuttle.Esb
{
	public interface IConfigurator
	{
		void Apply(IServiceBusConfiguration configuration);
	}
}