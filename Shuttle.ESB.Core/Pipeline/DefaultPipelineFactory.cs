using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
    public class DefaultPipelineFactory : IPipelineFactory
    {
        private readonly ReusableObjectPool<MessagePipeline> _pool;

        public DefaultPipelineFactory()
        {
            _pool = new ReusableObjectPool<MessagePipeline>();
        }

        private MessagePipeline CreatePipeline<TPipeline>(IServiceBus bus)
            where TPipeline : MessagePipeline
        {
            var messagePipeline = Activator.CreateInstance(typeof (TPipeline), new object[] {bus}) as MessagePipeline;

            bus.Events.OnPipelineCreated(this, new PipelineEventArgs(messagePipeline));

            return messagePipeline;
        }

        public TPipeline GetPipeline<TPipeline>(IServiceBus bus) where TPipeline : MessagePipeline
        {
            var messagePipeline = _pool.Get(typeof (TPipeline)) ?? CreatePipeline<TPipeline>(bus);

            messagePipeline.Obtained();

            return (TPipeline)messagePipeline;
        }

        public void ReleasePipeline(MessagePipeline messagePipeline)
        {
            Guard.AgainstNull(messagePipeline, "messagePipeline");

            _pool.Release(messagePipeline);

            messagePipeline.Released();
        }
    }
}