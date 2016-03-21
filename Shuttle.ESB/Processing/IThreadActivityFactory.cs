using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public interface IThreadActivityFactory
    {
        IThreadActivity CreateInboxThreadActivity(IServiceBus bus);
        IThreadActivity CreateControlInboxThreadActivity(IServiceBus bus);
        IThreadActivity CreateOutboxThreadActivity(IServiceBus bus);
    }
}