using System;

namespace Shuttle.Esb
{
    public interface IServiceBus :
        IMessageSender,
        IDisposable
    {
        bool Started { get; }

        IServiceBus Start();
        void Stop();
    }
}