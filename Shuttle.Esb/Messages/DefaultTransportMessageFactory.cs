using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class DefaultTransportMessageFactory : ITransportMessageFactory
    {
        private readonly IPipelineFactory _pipelineFactory;

        public DefaultTransportMessageFactory(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _pipelineFactory = pipelineFactory;
        }

        public TransportMessage Create(object message, Action<TransportMessageConfigurator> configure)
        {
            return Create(message, configure, null);
        }

        public TransportMessage Create(object message, TransportMessage transportMessageReceived)
        {
            return Create(message, null, transportMessageReceived);
        }

        public TransportMessage Create(object message, Action<TransportMessageConfigurator> configure,
            TransportMessage transportMessageReceived)
        {
            Guard.AgainstNull(message, nameof(message));

            var transportMessagePipeline = _pipelineFactory.GetPipeline<TransportMessagePipeline>();

            try
            {
                var transportMessageConfigurator = new TransportMessageConfigurator(message);

                if (transportMessageReceived != null)
                {
                    transportMessageConfigurator.TransportMessageReceived(transportMessageReceived);
                }

                configure?.Invoke(transportMessageConfigurator);

                if (!transportMessagePipeline.Execute(transportMessageConfigurator))
                {
                    throw new PipelineException(string.Format(Resources.PipelineExecutionException,
                            "TransportMessagePipeline", transportMessagePipeline.Exception.AllMessages()),
                        transportMessagePipeline.Exception);
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