using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public class DefaultConfigurator
    {
        private readonly IComponentRegistry _registry;
        private readonly List<Type> _dontRegisterTypes = new List<Type>();
        private readonly ILog _log;

        public DefaultConfigurator(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, "registry");

            _registry = registry;

            _log = Log.For(this);
        }

        public DefaultConfigurator DontRegister<TService>()
        {
            if (!_dontRegisterTypes.Contains(typeof (TService)))
            {
                _dontRegisterTypes.Add(typeof (TService));
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

            RegisterDefaultInstance(_registry, configuration);

            var transactionScopeConfiguration = configuration.TransactionScope;

            if (transactionScopeConfiguration == null)
            {
                _log.Warning(EsbResources.WarningNullTransactionScopeConfiguration);

                transactionScopeConfiguration = new TransactionScopeConfiguration();
            }

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

        private void RegisterDefault<TService, TImplementation>(IComponentRegistry registry)
            where TImplementation : class where TService : class
        {
            if (_dontRegisterTypes.Contains(typeof (TService)))
            {
                return;
            }

            registry.Register<TService, TImplementation>();
        }

        private void RegisterDefaultInstance<TService>(IComponentRegistry registry, TService instance)
            where TService : class
        {
            if (_dontRegisterTypes.Contains(typeof (TService)))
            {
                return;
            }

            registry.Register(instance);
        }

        public static IServiceBusConfiguration Configure(IComponentRegistry registry)
        {
            return new DefaultConfigurator(registry).Configure();
        }
    }
}