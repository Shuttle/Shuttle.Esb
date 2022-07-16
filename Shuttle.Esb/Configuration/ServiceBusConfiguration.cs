using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ServiceBusConfiguration : IServiceBusConfiguration
    {
        public IInboxConfiguration Inbox { get; set; }
        public IControlInboxConfiguration ControlInbox { get; set; }
        public IOutboxConfiguration Outbox { get; set; }
        public IWorkerConfiguration Worker { get; set; }
    }
}