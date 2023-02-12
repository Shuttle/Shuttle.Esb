using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IMessageHandler<in T> where T : class
    {
        Task ProcessMessage(IHandlerContext<T> context);
    }
}