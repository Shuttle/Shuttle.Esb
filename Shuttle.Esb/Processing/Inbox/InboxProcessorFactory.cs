using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class InboxProcessorFactory : IProcessorFactory
	{
		private readonly IServiceBus _bus;
	    private readonly IThreadActivityFactory _threadActivityFactory;
	    private readonly IWorkerAvailabilityManager _workerAvailabilityManager;
	    private readonly IPipelineFactory _pipelineFactory;

	    public InboxProcessorFactory(IServiceBus bus, IThreadActivityFactory threadActivityFactory, IWorkerAvailabilityManager workerAvailabilityManager, IPipelineFactory pipelineFactory)
		{
			Guard.AgainstNull(bus, "bus");
			Guard.AgainstNull(threadActivityFactory, "threadActivityFactory");
			Guard.AgainstNull(workerAvailabilityManager, "workerAvailabilityManager");
			Guard.AgainstNull(pipelineFactory, "pipelineFactory");

			_bus = bus;
	        _threadActivityFactory = threadActivityFactory;
	        _workerAvailabilityManager = workerAvailabilityManager;
	        _pipelineFactory = pipelineFactory;
		}

		public IProcessor Create()
		{
			return new InboxProcessor(_bus, _threadActivityFactory.CreateInboxThreadActivity(_bus), _workerAvailabilityManager, _pipelineFactory);
		}
	}
}