namespace Shuttle.Esb
{
	public class ControlInboxProcessor : QueueProcessor<ControlInboxMessagePipeline>
	{
		public ControlInboxProcessor(IServiceBus bus) :
			base(bus, bus.Configuration.ThreadActivityFactory.CreateControlInboxThreadActivity(bus))
		{
		}
	}
}