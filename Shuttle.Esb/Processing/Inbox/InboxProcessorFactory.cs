using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb
{
    public interface IInboxProcessorFactory : IProcessorFactory
    {
    }

    public class InboxProcessorFactory : IInboxProcessorFactory
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPipelineThreadActivity _pipelineThreadActivity;
        private readonly IWorkerAvailabilityService _workerAvailabilityService;
        private readonly ServiceBusOptions _options;

        public InboxProcessorFactory(ServiceBusOptions options,
            IWorkerAvailabilityService workerAvailabilityService, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
        {
            Guard.AgainstNull(options, nameof(options));
            Guard.AgainstNull(workerAvailabilityService, nameof(workerAvailabilityService));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(pipelineThreadActivity, nameof(pipelineThreadActivity));

            _options = options;
            _workerAvailabilityService = workerAvailabilityService;
            _pipelineFactory = pipelineFactory;
            _pipelineThreadActivity = pipelineThreadActivity;
        }

        public IProcessor Create()
        {
            var threadActivity = new ThreadActivity(_options.Inbox?.DurationToSleepWhenIdle ?? ServiceBusOptions.DefaultDurationToSleepWhenIdle);

            return new InboxProcessor(_options, threadActivity, _workerAvailabilityService, _pipelineFactory, _pipelineThreadActivity);
        }
    }
}