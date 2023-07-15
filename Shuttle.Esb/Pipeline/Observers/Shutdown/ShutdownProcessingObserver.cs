using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public interface IShutdownProcessingObserver : IPipelineObserver<OnStopping>
    {
    }

    public class ShutdownProcessingObserver : IShutdownProcessingObserver
    {
        private readonly IQueueService _queueService;

        public ShutdownProcessingObserver(IQueueService queueService)
        {
            Guard.AgainstNull(queueService, nameof(queueService));

            _queueService = queueService;
        }

        public async Task Execute(OnStopping pipelineEvent)
        {
            _queueService.TryDispose();

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}