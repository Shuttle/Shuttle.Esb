namespace Shuttle.Esb
{
    public interface IQueue : IBrokerEndpoint
    {
        void Acknowledge(object acknowledgementToken);
        void Release(object acknowledgementToken);
    }
}