using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class InboxProcessorFactory : IProcessorFactory
	{
		private readonly IServiceBus _bus;

		public InboxProcessorFactory(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "bus");

			_bus = bus;
		}

		public IProcessor Create()
		{
			return new InboxProcessor(_bus, _bus.Configuration.ThreadActivityFactory.CreateInboxThreadActivity(_bus));
		}
	}
}