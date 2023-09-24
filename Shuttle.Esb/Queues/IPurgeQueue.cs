using System.Threading.Tasks;

namespace Shuttle.Esb
{
    public interface IPurgeQueue
    {
        void Purge();
        Task PurgeAsync();
    }
}