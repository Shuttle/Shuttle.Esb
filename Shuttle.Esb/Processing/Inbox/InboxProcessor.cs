using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public class InboxProcessor : IProcessor
{
    private readonly IPipelineFactory _pipelineFactory;
    private readonly IPipelineThreadActivity _pipelineThreadActivity;
    private readonly IThreadActivity _threadActivity;

    public InboxProcessor(IThreadActivity threadActivity, IPipelineFactory pipelineFactory, IPipelineThreadActivity pipelineThreadActivity)
    {
        _threadActivity = Guard.AgainstNull(threadActivity);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _pipelineThreadActivity = Guard.AgainstNull(pipelineThreadActivity);
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var messagePipeline = _pipelineFactory.GetPipeline<InboxMessagePipeline>();

        try
        {
            messagePipeline.State.ResetWorking();
            messagePipeline.State.SetTransportMessage(null);
            messagePipeline.State.SetReceivedMessage(null);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

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