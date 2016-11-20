using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultConfigurator
    {
        private readonly ServiceBusConfiguration _configuration = new ServiceBusConfiguration();

        public DefaultConfigurator ComponentContainer(IComponentContainer container)
        {
            Guard.AgainstNull(container, "container");

            Container = container;

            return this;
        }

        public DefaultConfigurator AddCompressionAlgorithm(ICompressionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, "algorithm");

            _configuration.AddCompressionAlgorithm(algorithm);

            return this;
        }

        public DefaultConfigurator AddEnryptionAlgorithm(IEncryptionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, "algorithm");

            _configuration.AddEncryptionAlgorithm(algorithm);

            return this;
        }

        public IComponentContainer Container { get; private set; }

        public IServiceBusConfiguration Configuration()
        {
            if (Container == null)
            {
                Container = new DefaultComponentContainer();
            }

            new CoreConfigurator().Apply(_configuration);
            new ControlInboxConfigurator().Apply(_configuration);
            new InboxConfigurator().Apply(_configuration);
            new OutboxConfigurator().Apply(_configuration);
            new WorkerConfigurator().Apply(_configuration);

            RegisterComponents(Container);

            return _configuration;
        }

        public void RegisterComponents(IComponentContainer container)
        {
            Guard.AgainstNull(container, "container");

            ApplyDefault<ISerializer, DefaultSerializer>(container);
            ApplyDefault<IServiceBusPolicy, DefaultServiceBusPolicy>(container);
            ApplyDefault<IMessageRouteProvider, DefaultMessageRouteProvider>(container);
            ApplyDefault<IIdentityProvider, DefaultIdentityProvider>(container);
            ApplyDefault<IMessageHandlerInvoker, DefaultMessageHandlerInvoker>(container);
            ApplyDefault<IMessageHandlingAssessor, DefaultMessageHandlingAssessor>(container);
            ApplyDefault<IThreadActivityFactory, DefaultThreadActivityFactory>(container);
            ApplyDefault<IUriResolver, DefaultUriResolver>(container);
            ApplyDefault<IQueueManager, QueueManager>(container);
            ApplyDefault<IWorkerAvailabilityManager, WorkerAvailabilityManager>(container);
            ApplyDefault<ISubscriptionService, NullSubscriptionService>(container);
            ApplyDefault<IIdempotenceService, NullIdempotenceService>(container);

            container.Register<IServiceBusConfiguration>(_configuration);

            if (!container.IsRegistered(typeof(IPipelineFactory)))
            {
                container.Register<ITransactionScopeFactory>(new DefaultTransactionScopeFactory(_configuration.TransactionScope.Enabled, _configuration.TransactionScope.IsolationLevel, TimeSpan.FromSeconds(_configuration.TransactionScope.TimeoutSeconds)));
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

            if (_configuration.RegisterHandlers)
            {
                container.RegisterMessageHandlers();
            }
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