namespace Shuttle.ESB.Core
{
	public interface IConfigurator
	{
		void Apply(IServiceBusConfiguration configuration);
	}
}