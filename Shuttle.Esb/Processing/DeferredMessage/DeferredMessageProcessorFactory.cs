using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class DeferredMessageProcessorFactory : IProcessorFactory
    {
        private static readonly object Padlock = new object();
        private readonly IServiceBusConfiguration _configuration;
        private bool _instanced;

        public DeferredMessageProcessorFactory(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
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

                return _configuration.Inbox.DeferredMessageProcessor;
            }
        }
    }
}