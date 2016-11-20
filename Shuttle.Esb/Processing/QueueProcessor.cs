using System.Diagnostics;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public abstract class QueueProcessor<TPipeline> : IProcessor
		where TPipeline : IPipeline
	{
		protected readonly IServiceBus Bus;
		protected readonly IThreadActivity ThreadActivity;
	    private readonly IPipelineFactory _pipelineFactory;

	    protected QueueProcessor(IServiceBus bus, IThreadActivity threadActivity, IPipelineFactory pipelineFactory)
		{
			Guard.AgainstNull(bus, "bus");
			Guard.AgainstNull(threadActivity, "threadActivity");
			Guard.AgainstNull(pipelineFactory, "pipelineFactory");

			Bus = bus;
			ThreadActivity = threadActivity;
	        _pipelineFactory = pipelineFactory;
		}

		public virtual void Execute(IThreadState state)
		{
			var messagePipeline = _pipelineFactory.GetPipeline<TPipeline>();

			try
			{
				messagePipeline.State.ResetWorking();
				messagePipeline.State.Replace(StateKeys.ActiveState, state);

				messagePipeline.Execute();

				if (messagePipeline.State.GetWorking())
				{
					Bus.Events.OnThreadWorking(this, new ThreadStateEventArgs(typeof (TPipeline)));

					ThreadActivity.Working();
				}
				else
				{
					Bus.Events.OnThreadWaiting(this, new ThreadStateEventArgs(typeof (TPipeline)));

					ThreadActivity.Waiting(state);
				}
			}
			finally
			{
				_pipelineFactory.ReleasePipeline(messagePipeline);
			}
		}

		[DebuggerNonUserCode]
		void IProcessor.Execute(IThreadState state)
		{
			Execute(state);
		}
	}
}