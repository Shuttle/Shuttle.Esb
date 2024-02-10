using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ServiceBusHostedService : IHostedService
    {
        private readonly IServiceBus _serviceBus;
        private readonly ServiceBusOptions _serviceBusOptions;

        public ServiceBusHostedService(IOptions<ServiceBusOptions> serviceBusOptions, IServiceBus serviceBus)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            
            _serviceBusOptions = Guard.AgainstNull(serviceBusOptions.Value, nameof(serviceBusOptions.Value));
            _serviceBus = Guard.AgainstNull(serviceBus, nameof(serviceBus));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_serviceBusOptions.Asynchronous)
            {
                await _serviceBus.StartAsync();
            }
            else
            {
                _serviceBus.Start();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_serviceBusOptions.Asynchronous)
            {
                await _serviceBus.StopAsync();
            }
            else
            {
                _serviceBus.Stop();
            }
        }
    }
}