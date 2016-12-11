using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultTransportMessageFactory : ITransportMessageFactory
    {
        private readonly IPipelineFactory _pipelineFactory;

        public DefaultTransportMessageFactory(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");

            _pipelineFactory = pipelineFactory;
        }

        public TransportMessage Create(object message, Action<TransportMessageConfigurator> configure)
        {
            Guard.AgainstNull(message, "message");

            var transportMessagePipeline = _pipelineFactory.GetPipeline<TransportMessagePipeline>();

            try
            {
                var transportMessageConfigurator = new TransportMessageConfigurator(message);

                if (configure != null)
                {
                    configure(transportMessageConfigurator);
                }

                if (!transportMessagePipeline.Execute(transportMessageConfigurator))
                {
                    throw new PipelineException(string.Format(EsbResources.PipelineExecutionException,
                        "TransportMessagePipeline", transportMessagePipeline.Exception.AllMessages()));
                }

                return transportMessagePipeline.State.GetTransportMessage();
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(transportMessagePipeline);
            }
        }
    }
}