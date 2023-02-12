using System.IO;
using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IQueue
    {
        QueueUri Uri { get; }
        bool IsStream { get; }
        Task<bool> IsEmpty();
        Task Enqueue(TransportMessage message, Stream stream);
        Task<ReceivedMessage> GetMessage();
        Task Acknowledge(object acknowledgementToken);
        Task Release(object acknowledgementToken);
    }
}