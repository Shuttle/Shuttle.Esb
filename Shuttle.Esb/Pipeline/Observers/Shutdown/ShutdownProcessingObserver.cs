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

        public void Execute(OnStopping pipelineEvent)
        {
            _queueService.TryDispose();
        }

        public async Task ExecuteAsync(OnStopping pipelineEvent)
        {
            _queueService.TryDispose();

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}