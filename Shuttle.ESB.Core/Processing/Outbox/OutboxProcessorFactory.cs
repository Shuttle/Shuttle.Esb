using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class OutboxProcessorFactory : IProcessorFactory
	{
		private readonly IServiceBus _bus;

		public OutboxProcessorFactory(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "bus");

			_bus = bus;
		}

		public IProcessor Create()
		{
			return new OutboxProcessor(_bus);
		}
	}
}