using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public interface IServiceBusConfiguration
    {
        IControlInboxConfiguration ControlInbox { get; }
        IInboxConfiguration Inbox { get; }
        IOutboxConfiguration Outbox { get; }
        IWorkerConfiguration Worker { get; }
    }
}