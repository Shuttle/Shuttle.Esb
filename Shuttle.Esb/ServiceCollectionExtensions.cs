using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Serialization;
using Shuttle.Core.System;
using Shuttle.Core.Threading;
using Shuttle.Core.Transactions;

namespace Shuttle.Esb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceBus(this IServiceCollection services,
            Action<ServiceBusBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var serviceBusBuilder = new ServiceBusBuilder(services);

            builder?.Invoke(serviceBusBuilder);

            services.TryAddSingleton<IEnvironmentService, EnvironmentService>();
            services.TryAddSingleton<IProcessService, ProcessService>();
            services.TryAddSingleton<ISerializer, DefaultSerializer>();
            services.TryAddSingleton<IServiceBusPolicy, DefaultServiceBusPolicy>();
            services.TryAddSingleton<IMessageRouteProvider, DefaultMessageRouteProvider>();
            services.TryAddSingleton<IIdentityProvider, DefaultIdentityProvider>();
            services.TryAddSingleton<IMessageHandlerInvoker, DefaultMessageHandlerInvoker>();
            services.TryAddSingleton<IMessageHandlingAssessor, DefaultMessageHandlingAssessor>();
            services.TryAddSingleton<IUriResolver, DefaultUriResolver>();
            services.TryAddSingleton<IQueueService, QueueService>();
            services.TryAddSingleton<IQueueFactoryService, QueueFactoryService>();
            services.TryAddSingleton<IWorkerAvailabilityService, WorkerAvailabilityService>();
            services.TryAddSingleton<ISubscriptionService, NullSubscriptionService>();
            services.TryAddSingleton<IIdempotenceService, NullIdempotenceService>();
            services.TryAddSingleton<ITransactionScopeObserver, TransactionScopeObserver>();
            services.TryAddSingleton<ICancellationTokenSource, DefaultCancellationTokenSource>();
            services.TryAddSingleton<IPipelineThreadActivity, PipelineThreadActivity>();
            services.TryAddSingleton<IEncryptionService, EncryptionService>();
            services.TryAddSingleton<ICompressionService, CompressionService>();

            var transactionScopeFactoryType = typeof(ITransactionScopeFactory);

            if (services.All(item => item.ServiceType != transactionScopeFactoryType))
            {
                services.AddTransactionScope();
            }

            services.TryAddSingleton<ITransportMessageFactory, TransportMessageFactory>();

            services.AddPipelineProcessing(typeof(ServiceBus).Assembly);
            services.AddPipelineTransaction();

            services.AddOptions<ServiceBusOptions>().Configure(options =>
            {
                options.Inbox = serviceBusBuilder.Options.Inbox;
                options.Outbox = serviceBusBuilder.Options.Outbox;
                options.ControlInbox = serviceBusBuilder.Options.ControlInbox;

                ApplyDefaults(options.Inbox);
                ApplyDefaults(options.Outbox);
                ApplyDefaults(options.ControlInbox);

                options.Worker = serviceBusBuilder.Options.Worker;

                options.AddMessageHandlers = serviceBusBuilder.Options.AddMessageHandlers;
                options.CacheIdentity = serviceBusBuilder.Options.CacheIdentity;
                options.CompressionAlgorithm = serviceBusBuilder.Options.CompressionAlgorithm;
                options.CreatePhysicalQueues = serviceBusBuilder.Options.CreatePhysicalQueues;
                options.EncryptionAlgorithm = serviceBusBuilder.Options.EncryptionAlgorithm;
                options.RemoveCorruptMessages = serviceBusBuilder.Options.RemoveCorruptMessages;

                options.UriMappings = serviceBusBuilder.Options.UriMappings;
                options.MessageRoutes = serviceBusBuilder.Options.MessageRoutes;
            });

            var serviceBusConfigurationType = typeof(IServiceBusConfiguration);

            if (services.All(item => item.ServiceType != serviceBusConfigurationType))
            {
                services.AddSingleton<IServiceBusConfiguration>(serviceBusBuilder.Configuration);
            }

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

            services.AddSingleton<IServiceBus, ServiceBus>();

            return services;
        }

        private static void ApplyDefaults(ProcessorOptions processorOptions)
        {
            if (processorOptions == null)
            {
                return;
            }
            
            if (!(processorOptions.DurationToIgnoreOnFailure ?? Enumerable.Empty<TimeSpan>()).Any())
            {
                processorOptions.DurationToIgnoreOnFailure = new List<TimeSpan>(ServiceBusOptions.DefaultDurationToIgnoreOnFailure);
            }

            if (!(processorOptions.DurationToSleepWhenIdle ?? Enumerable.Empty<TimeSpan>()).Any())
            {
                processorOptions.DurationToSleepWhenIdle = new List<TimeSpan>(ServiceBusOptions.DefaultDurationToSleepWhenIdle);
            }
        }
    }
}