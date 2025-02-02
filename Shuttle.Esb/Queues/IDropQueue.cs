using System.Threading.Tasks;

namespace Shuttle.Esb;

public interface IDropQueue
{
    Task DropAsync();
}