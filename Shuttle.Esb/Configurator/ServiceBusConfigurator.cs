using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class ServiceBusConfigurator
    {
        private readonly IComponentRegistry _registry;
        private readonly List<Type> _dontRegisterTypes = new List<Type>();

        public ServiceBusConfigurator(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, "registry");

            _registry = registry;
        }

        public ServiceBusConfigurator DontRegister<TDependency>()
        {
            if (!_dontRegisterTypes.Contains(typeof (TDependency)))
            {
                _dontRegisterTypes.Add(typeof (TDependency));
            }

            return this;
        }

        public IServiceBusConfiguration Configure()
        {
            var configuration = new ServiceBusConfiguration();

            new CoreConfigurator().Apply(configuration);
            new UriResolverConfigurator().Apply(configuration);
            new QueueManagerConfigurator().Apply(configuration);
            new MessageRouteConfigurator().Apply(configuration);
            new ControlInboxConfigurator().Apply(configuration);
            new InboxConfigurator().Apply(configuration);
            new OutboxConfigurator().Apply(configuration);
            new WorkerConfigurator().Apply(configuration);

            RegisterComponents(configuration);

            return configuration;
        }

        public void RegisterComponents(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            RegisterDefaultInstance(_registry, configuration);

            RegisterDefault<IServiceBusEvents, ServiceBusEvents>(_registry);
            RegisterDefault<ISerializer, DefaultSerializer>(_registry);
            RegisterDefault<IServiceBusPolicy, DefaultServiceBusPolicy>(_registry);
            RegisterDefault<IMessageRouteProvider, DefaultMessageRouteProvider>(_registry);
            RegisterDefault<IIdentityProvider, DefaultIdentityProvider>(_registry);
            RegisterDefault<IMessageHandlerInvoker, DefaultMessageHandlerInvoker>(_registry);
            RegisterDefault<IMessageHandlingAssessor, DefaultMessageHandlingAssessor>(_registry);
            RegisterDefault<IUriResolver, DefaultUriResolver>(_registry);
            RegisterDefault<IQueueManager, QueueManager>(_registry);
            RegisterDefault<IWorkerAvailabilityManager, WorkerAvailabilityManager>(_registry);
            RegisterDefault<ISubscriptionManager, NullSubscriptionManager>(_registry);
            RegisterDefault<IIdempotenceService, NullIdempotenceService>(_registry);
            
            var transactionScopeConfiguration = configuration.TransactionScope ?? new TransactionScopeConfiguration();

            RegisterDefaultInstance<ITransactionScopeFactory>(_registry,
                new DefaultTransactionScopeFactory(transactionScopeConfiguration.Enabled,
                    transactionScopeConfiguration.IsolationLevel,
                    TimeSpan.FromSeconds(transactionScopeConfiguration.TimeoutSeconds)));

            RegisterDefault<ITransportMessageFactory, DefaultTransportMessageFactory>(_registry);
            RegisterDefault<IPipelineFactory, DefaultPipelineFactory>(_registry);

            _registry.Register(typeof (StartupPipeline), typeof (StartupPipeline), Lifestyle.Transient);
            _registry.Register(typeof (InboxMessagePipeline), typeof (InboxMessagePipeline),
                Lifestyle.Transient);
            _registry.Register(typeof (DistributorPipeline), typeof (DistributorPipeline), Lifestyle.Transient);
            _registry.Register(typeof (DispatchTransportMessagePipeline),
                typeof (DispatchTransportMessagePipeline), Lifestyle.Transient);
            _registry.Register(typeof (DeferredMessagePipeline), typeof (DeferredMessagePipeline),
                Lifestyle.Transient);
            _registry.Register(typeof (OutboxPipeline), typeof (OutboxPipeline), Lifestyle.Transient);
            _registry.Register(typeof (TransportMessagePipeline), typeof (TransportMessagePipeline),
                Lifestyle.Transient);
            _registry.Register(typeof (ControlInboxMessagePipeline), typeof (ControlInboxMessagePipeline),
                Lifestyle.Transient);

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypes<IPipelineObserver>())
            {
                if (type.IsInterface || _dontRegisterTypes.Contains(type))
                {
                    continue;
                }

                _registry.Register(type, type, Lifestyle.Singleton);
            }

            if (configuration.RegisterHandlers)
            {
                _registry.RegisterMessageHandlers();
            }

            var queueFactoryType = typeof(IQueueFactory);
            var queueFactoryImplementationTypes = new List<Type>();
            Action<Type> addQueueFactoryImplementationType = (Type type) =>
            {
                if (queueFactoryImplementationTypes.Contains(type))
                {
                    return;
                }

                queueFactoryImplementationTypes.Add(type);
            };

            if (configuration.ScanForQueueFactories)
            {
                foreach (var type in new ReflectionService().GetTypes<IQueueFactory>())
                {
                    addQueueFactoryImplementationType(type);
                }
            }

            foreach (var type in configuration.QueueFactoryTypes)
            {
                addQueueFactoryImplementationType(type);
            }

            _registry.RegisterCollection(queueFactoryType, queueFactoryImplementationTypes, Lifestyle.Singleton);

            _registry.Register<IServiceBus, ServiceBus>();
        }

        public static IServiceBusConfiguration Configure(IComponentRegistry registry)
        {
            return new ServiceBusConfigurator(registry).Configure();
        }

        private void RegisterDefault<TDependency, TImplementation>(IComponentRegistry registry)
            where TDependency : class
            where TImplementation : class, TDependency
        {
            if (_dontRegisterTypes.Contains(typeof(TDependency)))
            {
                return;
            }

            registry.Register<TDependency, TImplementation>();
        }

        private void RegisterDefaultInstance<TDependency>(IComponentRegistry registry, TDependency instance)
            where TDependency : class
        {
            if (_dontRegisterTypes.Contains(typeof(TDependency)))
            {
                return;
            }

            registry.Register(instance);
        }
    }
}