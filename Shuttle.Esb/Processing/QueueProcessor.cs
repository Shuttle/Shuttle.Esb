using System.Diagnostics;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public abstract class QueueProcessor<TPipeline> : IProcessor
		where TPipeline : IPipeline
	{
		protected readonly IServiceBus Bus;
		protected readonly IThreadActivity ThreadActivity;

		protected QueueProcessor(IServiceBus bus, IThreadActivity threadActivity)
		{
			Guard.AgainstNull(bus, "bus");
			Guard.AgainstNull(threadActivity, "threadActivity");

			Bus = bus;
			ThreadActivity = threadActivity;
		}

		public virtual void Execute(IThreadState state)
		{
			var messagePipeline = Bus.Configuration.PipelineFactory.GetPipeline<TPipeline>();

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
				Bus.Configuration.PipelineFactory.ReleasePipeline(messagePipeline);
			}
		}

		[DebuggerNonUserCode]
		void IProcessor.Execute(IThreadState state)
		{
			Execute(state);
		}
	}
}