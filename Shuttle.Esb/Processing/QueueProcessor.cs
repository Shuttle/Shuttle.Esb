using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public abstract class QueueProcessor<TPipeline> : IProcessor
    where TPipeline : IPipeline
{
    private readonly IPipelineFactory _pipelineFactory;
    private readonly IPipelineThreadActivity _pipelineThreadActivity;
    private readonly IThreadActivity _threadActivity;

    protected QueueProcessor(IThreadActivity threadActivity, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
    {
        _threadActivity = Guard.AgainstNull(threadActivity);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _pipelineThreadActivity = Guard.AgainstNull(pipelineThreadActivity);
    }

    public async Task ExecuteAsync(IProcessorThreadContext _, CancellationToken cancellationToken = default)
    {
        var messagePipeline = _pipelineFactory.GetPipeline<TPipeline>();

        try
        {
            messagePipeline.State.ResetWorking();

            await messagePipeline.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            if (messagePipeline.State.GetWorking())
            {
                _threadActivity.Working();

                _pipelineThreadActivity.OnThreadWorking(this, new(messagePipeline));
            }
            else
            {
                _pipelineThreadActivity.OnThreadWaiting(this, new(messagePipeline));

                await _threadActivity.WaitingAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _pipelineFactory.ReleasePipeline(messagePipeline);
        }
    }
}