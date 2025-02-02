using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public class DeferredMessageProcessorFactory : IProcessorFactory
{
    private static readonly object Padlock = new();
    private readonly IDeferredMessageProcessor _deferredMessageProcessor;
    private bool _instanced;

    public DeferredMessageProcessorFactory(IDeferredMessageProcessor deferredMessageProcessor)
    {
        _deferredMessageProcessor = Guard.AgainstNull(deferredMessageProcessor);
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