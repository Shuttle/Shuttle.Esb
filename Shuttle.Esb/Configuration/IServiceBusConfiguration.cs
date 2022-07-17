namespace Shuttle.Esb
{
    public interface IServiceBusConfiguration
    {
        IControlInboxConfiguration ControlInbox { get; set; }
        IInboxConfiguration Inbox { get; set; }
        IOutboxConfiguration Outbox { get; set; }
        IWorkerConfiguration Worker { get; set; }
    }
}