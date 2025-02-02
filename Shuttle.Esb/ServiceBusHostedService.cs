using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class ServiceBusHostedService : IHostedService
{
    private readonly IServiceBus _serviceBus;

    public ServiceBusHostedService(IServiceBus serviceBus)
    {
        _serviceBus = Guard.AgainstNull(serviceBus);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _serviceBus.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _serviceBus.StopAsync();
    }
}