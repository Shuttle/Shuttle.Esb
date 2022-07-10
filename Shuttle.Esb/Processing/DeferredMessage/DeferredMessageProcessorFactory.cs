using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class DeferredMessageProcessorFactory : IProcessorFactory
    {
        private static readonly object Padlock = new object();
        private readonly IServiceBusConfiguration _serviceBusConfiguration;
        private bool _instanced;

        public DeferredMessageProcessorFactory(IServiceBusConfiguration serviceBusConfiguration)
        {
            Guard.AgainstNull(serviceBusConfiguration, nameof(serviceBusConfiguration));

            _serviceBusConfiguration = serviceBusConfiguration;
        }

        public IProcessor Create()
        {
            lock (Padlock)
            {
                if (_instanced)
                {
                    throw new ProcessorException(Resources.DeferredMessageProcessorInstanceException);
                }

                _instanced = true;

                return _serviceBusConfiguration.Inbox.DeferredMessageProcessor;
            }
        }
    }
}