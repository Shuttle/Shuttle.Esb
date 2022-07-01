namespace Shuttle.Esb
{
    public interface IWorkConfiguration : IThreadCount
    {
        IBrokerEndpoint BrokerEndpoint { get; set; }
        string Uri { get; }
    }
}