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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _serviceBus.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _serviceBus.Stop();

            return Task.CompletedTask;
        }
    }
}