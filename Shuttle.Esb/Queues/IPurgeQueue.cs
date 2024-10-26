using System.Threading.Tasks;

namespace Shuttle.Esb;

public interface IPurgeQueue
{
    Task PurgeAsync();
}