namespace Shuttle.Esb;

public interface IServiceBusConfiguration
{
    IInboxConfiguration? Inbox { get; }
    IOutboxConfiguration? Outbox { get; }
}