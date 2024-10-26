using System.Threading.Tasks;

namespace Shuttle.Esb;

public interface IDropQueue
{
    void Drop();
    Task DropAsync();
}