using System.Diagnostics;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public abstract class QueueProcessor<TMessagePipeline> : IProcessor
        where TMessagePipeline : MessagePipeline
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
            var messagePipeline = Bus.Configuration.PipelineFactory.GetPipeline<TMessagePipeline>(Bus);

            try
            {
                messagePipeline.State.ResetWorking();
                messagePipeline.State.Replace(StateKeys.ActiveState, state);

                messagePipeline.Execute();

                if (messagePipeline.State.GetWorking())
                {
                    Bus.Events.OnThreadWorking(this, new ThreadStateEventArgs(typeof(TMessagePipeline)));

                    ThreadActivity.Working();
                }
                else
                {
                    Bus.Events.OnThreadWaiting(this, new ThreadStateEventArgs(typeof(TMessagePipeline)));

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