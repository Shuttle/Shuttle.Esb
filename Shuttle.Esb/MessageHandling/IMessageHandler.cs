namespace Shuttle.Esb
{
    public interface IMessageHandler<in T> where T : class
    {
        void ProcessMessage(IHandlerContext<T> context);
    }
}