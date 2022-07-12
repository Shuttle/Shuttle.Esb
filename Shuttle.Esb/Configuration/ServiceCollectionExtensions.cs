using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;
using Shuttle.Core.System;
using Shuttle.Core.Threading;
using Shuttle.Core.Transactions;

namespace Shuttle.Esb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceBus(this IServiceCollection services, Action<ServiceBusBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));
            Guard.AgainstNull(builder, nameof(builder));

            var configurationBuilder = new ServiceBusBuilder(services);

            builder?.Invoke(configurationBuilder);

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

            services.TryAddSingleton<IPipelineFactory, PipelineFactory>();
            services.TryAddSingleton<ITransportMessageFactory, TransportMessageFactory>();

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipeline>(typeof(ServiceBus).Assembly))
            {
                if (type.IsInterface || type.IsAbstract || services.Contains(ServiceDescriptor.Transient(type, type)))
                {
                    continue;
                }

                services.AddTransient(type, type);
            }

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipelineObserver>(typeof(ServiceBus).Assembly))
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                var interfaceType = type.InterfaceMatching($"I{type.Name}");

                if (interfaceType != null)
                {
                    if (services.Contains(ServiceDescriptor.Transient(interfaceType, type)))
                    {
                        continue;
                    }

                    services.AddTransient(interfaceType, type);
                }
                else
                {
                    throw new EsbConfigurationException(string.Format(Resources.ObserverInterfaceMissingException,
                        type.Name));
                }
            }

            services.AddOptions<ServiceBusOptions>().Configure(options =>
            {
                options.Inbox = configurationBuilder.Options.Inbox;
                options.Outbox = configurationBuilder.Options.Outbox;
                options.ControlInbox = configurationBuilder.Options.ControlInbox;

                options.Worker = configurationBuilder.Options.Worker;

                options.AddMessageHandlers = configurationBuilder.Options.AddMessageHandlers;
                options.CacheIdentity = configurationBuilder.Options.CacheIdentity;
                options.CompressionAlgorithm = configurationBuilder.Options.CompressionAlgorithm;
                options.CreatePhysicalQueues = configurationBuilder.Options.CreatePhysicalQueues;
                options.EncryptionAlgorithm = configurationBuilder.Options.EncryptionAlgorithm;
                options.RemoveCorruptMessages = configurationBuilder.Options.RemoveCorruptMessages;

                options.UriMappings = configurationBuilder.Options.UriMappings;
                options.MessageRoutes = configurationBuilder.Options.MessageRoutes;
            });

            var serviceBusConfigurationType = typeof(IServiceBusConfiguration);
            
            if (services.All(item => item.ServiceType != serviceBusConfigurationType))
            {
                services.AddSingleton(configurationBuilder.GetConfiguration());
            }

            services.AddSingleton<IServiceBus, ServiceBus>();

            return services;
        }
    }
}