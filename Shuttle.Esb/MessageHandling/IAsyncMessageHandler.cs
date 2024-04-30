using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IAsyncMessageHandler<in T> where T : class
    {
        Task ProcessMessageAsync(IHandlerContext<T> context);
    }
}