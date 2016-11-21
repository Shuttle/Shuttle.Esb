using System.Diagnostics;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public abstract class QueueProcessor<TPipeline> : IProcessor
		where TPipeline : IPipeline
	{
	    private readonly IServiceBusEvents _events;
	    private readonly IThreadActivity _threadActivity;
	    private readonly IPipelineFactory _pipelineFactory;

	    protected QueueProcessor(IServiceBusEvents events, IThreadActivity threadActivity, IPipelineFactory pipelineFactory)
		{
			Guard.AgainstNull(events, "events");
			Guard.AgainstNull(threadActivity, "threadActivity");
			Guard.AgainstNull(pipelineFactory, "pipelineFactory");

	        _events = events;
	        _threadActivity = threadActivity;
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
					_events.OnThreadWorking(this, new ThreadStateEventArgs(typeof (TPipeline)));

					_threadActivity.Working();
				}
				else
				{
					_events.OnThreadWaiting(this, new ThreadStateEventArgs(typeof (TPipeline)));

					_threadActivity.Waiting(state);
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