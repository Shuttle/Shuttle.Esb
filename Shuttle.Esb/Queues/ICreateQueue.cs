using System.Threading.Tasks;

namespace Shuttle.Esb;

public interface ICreateQueue
{
    Task CreateAsync();
}