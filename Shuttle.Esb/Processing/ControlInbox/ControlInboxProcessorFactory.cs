using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ControlInboxProcessorFactory : IProcessorFactory
	{
		private readonly IServiceBus _bus;
	    private readonly IThreadActivityFactory _threadActivityFactory;
	    private readonly IPipelineFactory _pipelineFactory;

	    public ControlInboxProcessorFactory(IServiceBus bus, IThreadActivityFactory threadActivityFactory, IPipelineFactory pipelineFactory)
		{
			Guard.AgainstNull(bus, "bus");
			Guard.AgainstNull(threadActivityFactory, "threadActivityFactory");
			Guard.AgainstNull(pipelineFactory, "bus");

			_bus = bus;
	        _threadActivityFactory = threadActivityFactory;
	        _pipelineFactory = pipelineFactory;
		}

		public IProcessor Create()
		{
			return new ControlInboxProcessor(_bus, _threadActivityFactory.CreateControlInboxThreadActivity(_bus), _pipelineFactory);
		}
	}
}