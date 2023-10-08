using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public class DeferredMessageProcessorFactory : IProcessorFactory
    {
        private readonly IDeferredMessageProcessor _deferredMessageProcessor;
        private static readonly object Padlock = new object();
        private bool _instanced;

        public DeferredMessageProcessorFactory(IDeferredMessageProcessor deferredMessageProcessor)
        {
            _deferredMessageProcessor = Guard.AgainstNull(deferredMessageProcessor, nameof(deferredMessageProcessor));
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

                return _deferredMessageProcessor;
            }
        }
    }
}