using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransactionScope;
using Shuttle.Core.Serialization;
using Shuttle.Core.System;
using Shuttle.Core.Threading;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Esb;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceBus(this IServiceCollection services, Action<ServiceBusBuilder>? builder = null)
    {
        var serviceBusBuilder = new ServiceBusBuilder(Guard.AgainstNull(services));

        builder?.Invoke(serviceBusBuilder);

        services.TryAddSingleton<IEnvironmentService, EnvironmentService>();
        services.TryAddSingleton<IProcessService, ProcessService>();
        services.TryAddSingleton<ISerializer, JsonSerializer>();
        services.TryAddSingleton<IServiceBusPolicy, DefaultServiceBusPolicy>();
        services.TryAddSingleton<IMessageRouteProvider, MessageRouteProvider>();
        services.TryAddSingleton<IIdentityProvider, DefaultIdentityProvider>();
        services.TryAddSingleton<IMessageHandlerInvoker, MessageHandlerInvoker>();
        services.TryAddSingleton<IMessageHandlingSpecification, MessageHandlingSpecification>();
        services.TryAddSingleton<IUriResolver, UriResolver>();
        services.TryAddSingleton<IQueueService, QueueService>();
        services.TryAddSingleton<IQueueFactoryService, QueueFactoryService>();
        services.TryAddSingleton<ISubscriptionService, NullSubscriptionService>();
        services.TryAddSingleton<IIdempotenceService, NullIdempotenceService>();
        services.TryAddSingleton<ICancellationTokenSource, DefaultCancellationTokenSource>();
        services.TryAddSingleton<IPipelineThreadActivity, PipelineThreadActivity>();
        services.TryAddSingleton<IEncryptionService, EncryptionService>();
        services.TryAddSingleton<ICompressionService, CompressionService>();
        services.TryAddSingleton<IDeferredMessageProcessor, DeferredMessageProcessor>();
        services.TryAddSingleton<IProcessorThreadPoolFactory, ProcessorThreadPoolFactory>();

        services.AddPipelineProcessing(pipelineProcessingBuilder =>
        {
            pipelineProcessingBuilder.AddAssembly(typeof(ServiceBus).Assembly);
        });

        var transactionScopeFactoryType = typeof(ITransactionScopeFactory);

        if (services.All(item => item.ServiceType != transactionScopeFactoryType))
        {
            services.AddTransactionScope();
        }

        if (!serviceBusBuilder.ShouldSuppressPipelineTransactionScope)
        {
            services.AddPipelineTransactionScope(transactionScopeBuilder =>
            {
                transactionScopeBuilder.AddStage<InboxMessagePipeline>("Handle");
            });
        }

        services.AddOptions<ServiceBusOptions>().Configure(options =>
        {
            options.Inbox = serviceBusBuilder.Options.Inbox;
            options.Outbox = serviceBusBuilder.Options.Outbox;

            ApplyDefaults(options.Inbox);
            ApplyDefaults(options.Outbox);

            options.AddMessageHandlers = serviceBusBuilder.Options.AddMessageHandlers;
            options.CacheIdentity = serviceBusBuilder.Options.CacheIdentity;
            options.CompressionAlgorithm = serviceBusBuilder.Options.CompressionAlgorithm;
            options.CreatePhysicalQueues = serviceBusBuilder.Options.CreatePhysicalQueues;
            options.EncryptionAlgorithm = serviceBusBuilder.Options.EncryptionAlgorithm;
            options.RemoveCorruptMessages = serviceBusBuilder.Options.RemoveCorruptMessages;

            options.UriMappings = serviceBusBuilder.Options.UriMappings;
            options.MessageRoutes = serviceBusBuilder.Options.MessageRoutes;
            options.Subscription = serviceBusBuilder.Options.Subscription;
            options.Idempotence = serviceBusBuilder.Options.Idempotence;
            options.ProcessorThread = serviceBusBuilder.Options.ProcessorThread;
        });

        services.AddSingleton<IServiceBusConfiguration, ServiceBusConfiguration>();
        services.AddSingleton<IMappedDelegateProvider>(_ => new MappedDelegateProvider(serviceBusBuilder.GetDelegates()));

        if (serviceBusBuilder.Options.AddMessageHandlers)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                serviceBusBuilder.AddMessageHandlers(assembly);
            }
        }
        else
        {
            serviceBusBuilder.AddMessageHandlers(typeof(ServiceBus).Assembly);
        }

        services.AddSingleton<IMessageSender, MessageSender>();
        services.AddSingleton<IServiceBus, ServiceBus>();

        if (!serviceBusBuilder.ShouldSuppressHostedService)
        {
            services.AddHostedService<ServiceBusHostedService>();
        }

        return services;
    }

    private static void ApplyDefaults(ProcessorOptions? processorOptions)
    {
        if (processorOptions == null)
        {
            return;
        }

        if (!processorOptions.DurationToIgnoreOnFailure.Any())
        {
            processorOptions.DurationToIgnoreOnFailure = new(ServiceBusOptions.DefaultDurationToIgnoreOnFailure);
        }

        if (!processorOptions.DurationToSleepWhenIdle.Any())
        {
            processorOptions.DurationToSleepWhenIdle = new(ServiceBusOptions.DefaultDurationToSleepWhenIdle);
        }
    }
}