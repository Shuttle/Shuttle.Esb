using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class OutboxProcessorFactory : IProcessorFactory
	{
		private readonly IServiceBus _bus;
	    private readonly IThreadActivityFactory _threadActivityFactory;
	    private readonly IPipelineFactory _pipelineFactory;

	    public OutboxProcessorFactory(IServiceBus bus, IThreadActivityFactory threadActivityFactory, IPipelineFactory pipelineFactory)
		{
			Guard.AgainstNull(bus, "bus");
			Guard.AgainstNull(threadActivityFactory, "threadActivityFactory");
			Guard.AgainstNull(pipelineFactory, "pipelineFactory");

			_bus = bus;
	        _threadActivityFactory = threadActivityFactory;
	        _pipelineFactory = pipelineFactory;
		}

		public IProcessor Create()
		{
			return new OutboxProcessor(_bus, _threadActivityFactory.CreateOutboxThreadActivity(_bus), _pipelineFactory);
		}
	}
}