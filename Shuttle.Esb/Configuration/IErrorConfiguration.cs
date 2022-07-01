namespace Shuttle.Esb
{
    public interface IErrorConfiguration
    {
        IBrokerEndpoint ErrorBrokerEndpoint { get; set; }
        string ErrorUri { get; }
    }
}