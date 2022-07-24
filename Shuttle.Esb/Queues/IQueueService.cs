namespace Shuttle.Esb
{
    public interface IQueueService
    {
        IQueue Get(string uri);
        bool Contains(string uri);
    }
}