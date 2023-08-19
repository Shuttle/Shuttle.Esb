using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ServiceBusHostedService : IHostedService
    {
        private readonly IServiceBus _serviceBus;

        public ServiceBusHostedService(IServiceBus serviceBus)
        {
            Guard.AgainstNull(serviceBus, nameof(serviceBus));

            _serviceBus = serviceBus;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _serviceBus.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _serviceBus.Stop();
        }
    }
}