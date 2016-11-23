using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ServiceBusConfigurator
    {
        public ServiceBusConfigurator()
        {
            Configuration = new ServiceBusConfiguration();
        }

        public ServiceBusConfigurator ComponentContainer(IComponentContainer container)
        {
            Guard.AgainstNull(container, "container");

            Container = container;

            return this;
        }

        public ServiceBusConfigurator AddCompressionAlgorithm(ICompressionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, "algorithm");

            Configuration.AddCompressionAlgorithm(algorithm);

            return this;
        }

        public ServiceBusConfigurator AddEnryptionAlgorithm(IEncryptionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, "algorithm");

            Configuration.AddEncryptionAlgorithm(algorithm);

            return this;
        }

        public IComponentContainer Container { get; private set; }
        public IServiceBusConfiguration Configuration { get; private set; } 

        public void Configure()
        {
            if (Container == null)
            {
                Container = new DefaultComponentContainer();
            }

            new CoreConfigurator().Apply(Configuration);
            new ControlInboxConfigurator().Apply(Configuration);
            new InboxConfigurator().Apply(Configuration);
            new OutboxConfigurator().Apply(Configuration);
            new WorkerConfigurator().Apply(Configuration);

            RegisterComponents(Container, Configuration);
        }

        public void RegisterComponents(IComponentContainer container, IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(container, "container");
            Guard.AgainstNull(configuration, "configuration");

            container.Register(container);

            ApplyDefault<IServiceBusEvents, ServiceBusEvents>(container);
            ApplyDefault<ISerializer, DefaultSerializer>(container);
            ApplyDefault<IServiceBusPolicy, DefaultServiceBusPolicy>(container);
            ApplyDefault<IMessageRouteProvider, DefaultMessageRouteProvider>(container);
            ApplyDefault<IIdentityProvider, DefaultIdentityProvider>(container);
            ApplyDefault<IMessageHandlerInvoker, DefaultMessageHandlerInvoker>(container);
            ApplyDefault<IMessageHandlingAssessor, DefaultMessageHandlingAssessor>(container);
            ApplyDefault<IUriResolver, DefaultUriResolver>(container);
            ApplyDefault<IQueueManager, QueueManager>(container);
            ApplyDefault<IWorkerAvailabilityManager, WorkerAvailabilityManager>(container);
            ApplyDefault<ISubscriptionManager, NullSubscriptionManager>(container);
            ApplyDefault<IIdempotenceService, NullIdempotenceService>(container);

            if (!container.IsRegistered(typeof(IServiceBusConfiguration)))
            {
                container.Register<IServiceBusConfiguration>(configuration);
            }

            if (!container.IsRegistered(typeof(IPipelineFactory)))
            {
                container.Register<ITransactionScopeFactory>(new DefaultTransactionScopeFactory(Configuration.TransactionScope.Enabled, Configuration.TransactionScope.IsolationLevel, TimeSpan.FromSeconds(Configuration.TransactionScope.TimeoutSeconds)));
            }

            if (!container.IsRegistered(typeof(IPipelineFactory)))
            {
                container.Register<IPipelineFactory>(new DefaultPipelineFactory(container));
            }

            container.Register(typeof(StartupPipeline), typeof(StartupPipeline), Lifestyle.Transient);
            container.Register(typeof(InboxMessagePipeline), typeof(InboxMessagePipeline), Lifestyle.Transient);
            container.Register(typeof(DistributorPipeline), typeof(DistributorPipeline), Lifestyle.Transient);
            container.Register(typeof(DispatchTransportMessagePipeline),
                typeof(DispatchTransportMessagePipeline), Lifestyle.Transient);
            container.Register(typeof(DeferredMessagePipeline), typeof(DeferredMessagePipeline),
                Lifestyle.Transient);
            container.Register(typeof(OutboxPipeline), typeof(OutboxPipeline), Lifestyle.Transient);
            container.Register(typeof(TransportMessagePipeline), typeof(TransportMessagePipeline),
                Lifestyle.Transient);
            container.Register(typeof(ControlInboxMessagePipeline), typeof(ControlInboxMessagePipeline),
                Lifestyle.Transient);

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypes<IPipelineObserver>())
            {
                if (type.IsInterface || container.IsRegistered(type))
                {
                    continue;
                }

                container.Register(type, type, Lifestyle.Singleton);
            }

            if (Configuration.RegisterHandlers)
            {
                container.RegisterMessageHandlers();
            }

            container.Register<IServiceBus, ServiceBus>();
        }

        private void ApplyDefault<TService, TImplementation>(IComponentContainer container) where TImplementation : class where TService : class
        {
            if (!container.IsRegistered(typeof(TService)))
            {
                container.Register<TService, TImplementation>();
            }
        }

    }
}