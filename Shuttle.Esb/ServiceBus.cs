using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Core.Threading;

namespace Shuttle.Esb;

public class ServiceBus : IServiceBus
{
    private readonly ICancellationTokenSource _cancellationTokenSource;
    private readonly IMessageSender _messageSender;
    private readonly IPipelineFactory _pipelineFactory;

    private IProcessorThreadPool? _controlInboxThreadPool;
    private IProcessorThreadPool? _deferredMessageThreadPool;
    private IProcessorThreadPool? _inboxThreadPool;
    private IProcessorThreadPool? _outboxThreadPool;

    private bool _disposed;

    public ServiceBus(IPipelineFactory pipelineFactory, IMessageSender messageSender, ICancellationTokenSource? cancellationTokenSource = null)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _messageSender = Guard.AgainstNull(messageSender);
    
        _cancellationTokenSource = cancellationTokenSource ?? new DefaultCancellationTokenSource();
    }

    public async Task<IServiceBus> StartAsync()
    {
        return await StartAsync(false).ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        if (!Started)
        {
            return;
        }

        _cancellationTokenSource.Renew();

        _deferredMessageThreadPool?.Dispose();
        _inboxThreadPool?.Dispose();
        _controlInboxThreadPool?.Dispose();
        _outboxThreadPool?.Dispose();

        await _pipelineFactory.GetPipeline<ShutdownPipeline>().ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

        _pipelineFactory.Flush();

        Started = false;
    }

    public bool Started { get; private set; }
    public bool Asynchronous { get; private set; }

    public async Task<TransportMessage> SendAsync(object message, Action<TransportMessageBuilder>? builder = null)
    {
        StartedGuard();

        return await _messageSender.SendAsync(message, null, builder).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TransportMessage>> PublishAsync(object message, Action<TransportMessageBuilder>? builder = null)
    {
        StartedGuard();

        return await _messageSender.PublishAsync(message, null, builder);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopAsync().GetAwaiter().GetResult();

        _cancellationTokenSource.TryDispose();

        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await StopAsync().ConfigureAwait(false);

        await _cancellationTokenSource.TryDisposeAsync();

        _disposed = true;
    }

    private async Task<IServiceBus> StartAsync(bool sync)
    {
        if (Started)
        {
            throw new ApplicationException(Resources.ServiceBusInstanceAlreadyStarted);
        }

        var startupPipeline = _pipelineFactory.GetPipeline<StartupPipeline>();

        Started = true; // required for using ServiceBus in OnStarted event
        Asynchronous = !sync;

        try
        {
            await startupPipeline.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

            _inboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("InboxThreadPool");
            _controlInboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("ControlInboxThreadPool");
            _outboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("OutboxThreadPool");
            _deferredMessageThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("DeferredMessageThreadPool");
        }
        catch
        {
            Started = false;
            throw;
        }

        return this;
    }

    private void StartedGuard()
    {
        if (Started)
        {
            return;
        }

        throw new InvalidOperationException(Resources.ServiceBusInstanceNotStarted);
    }
}