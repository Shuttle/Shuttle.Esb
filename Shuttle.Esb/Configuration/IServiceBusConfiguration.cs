namespace Shuttle.Esb
{
    public interface IServiceBusConfiguration
    {
        IControlInboxConfiguration ControlInbox { get; }
        IInboxConfiguration Inbox { get; }
        IOutboxConfiguration Outbox { get; }
        IWorkerConfiguration Worker { get; }
        void Configure(ServiceBusOptions serviceBusOptions);
    }
}