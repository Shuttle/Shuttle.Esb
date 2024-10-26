using System.Threading.Tasks;

namespace Shuttle.Esb;

public interface ICreateQueue
{
    void Create();
    Task CreateAsync();
}