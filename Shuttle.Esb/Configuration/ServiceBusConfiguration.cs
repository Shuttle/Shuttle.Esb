using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ServiceBusConfiguration : IServiceBusConfiguration
    {
        private readonly List<Type> _modules = new List<Type>();

        public IInboxConfiguration Inbox { get; set; }
        public IControlInboxConfiguration ControlInbox { get; set; }
        public IOutboxConfiguration Outbox { get; set; }
        public IWorkerConfiguration Worker { get; set; }

        public IEnumerable<Type> Modules => _modules.AsReadOnly();

        public void AddModule(Type type)
        {
            Guard.AgainstNull(type, nameof(type));

            _modules.Add(type);
        }
    }
}