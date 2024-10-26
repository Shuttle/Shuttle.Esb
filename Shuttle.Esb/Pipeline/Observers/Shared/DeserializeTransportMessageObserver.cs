using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;
using Shuttle.Core.System;

namespace Shuttle.Esb;

public interface IDeserializeTransportMessageObserver : IPipelineObserver<OnDeserializeTransportMessage>
{
    event EventHandler<DeserializationExceptionEventArgs>? TransportMessageDeserializationException;
}

public class DeserializeTransportMessageObserver : IDeserializeTransportMessageObserver
{
    private readonly IEnvironmentService _environmentService;
    private readonly IProcessService _processService;
    private readonly ISerializer _serializer;
    private readonly ServiceBusOptions _serviceBusOptions;

    public DeserializeTransportMessageObserver(IOptions<ServiceBusOptions> serviceBusOptions, ISerializer serializer, IEnvironmentService environmentService, IProcessService processService)
    {
        _serviceBusOptions = Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value);
        _serializer = Guard.AgainstNull(serializer);
        _environmentService = Guard.AgainstNull(environmentService);
        _processService = Guard.AgainstNull(processService);
    }

    public event EventHandler<DeserializationExceptionEventArgs>? TransportMessageDeserializationException;

    public async Task ExecuteAsync(IPipelineContext<OnDeserializeTransportMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var receivedMessage = Guard.AgainstNull(state.GetReceivedMessage());
        var workQueue = Guard.AgainstNull(state.GetWorkQueue());

        TransportMessage transportMessage;

        try
        {
            await using (var stream = await receivedMessage.Stream.CopyAsync().ConfigureAwait(false))
            {
                transportMessage = (TransportMessage)await _serializer.DeserializeAsync(typeof(TransportMessage), stream).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            TransportMessageDeserializationException?.Invoke(this, new(pipelineContext, workQueue, Guard.AgainstNull(state.GetErrorQueue()), ex));

            if (_serviceBusOptions.RemoveCorruptMessages)
            {
                await workQueue.AcknowledgeAsync(Guard.AgainstNull(state.GetReceivedMessage()).AcknowledgementToken).ConfigureAwait(false);
            }
            else
            {
                if (!_environmentService.UserInteractive)
                {
                    _processService.GetCurrentProcess().Kill();
                }

                return;
            }

            pipelineContext.Pipeline.Abort();

            return;
        }

        state.SetTransportMessage(transportMessage);
        state.SetMessageBytes(transportMessage.Message);

        transportMessage.AcceptInvariants();
    }
}