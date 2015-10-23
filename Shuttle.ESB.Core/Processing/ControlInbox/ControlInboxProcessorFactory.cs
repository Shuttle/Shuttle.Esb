using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class ControlInboxProcessorFactory : IProcessorFactory
	{
		private readonly IServiceBus _bus;

		public ControlInboxProcessorFactory(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "bus");

			_bus = bus;
		}

		public IProcessor Create()
		{
			return new ControlInboxProcessor(_bus);
		}
	}
}