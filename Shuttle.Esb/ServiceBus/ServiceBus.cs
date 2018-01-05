using System;
using System.Collections.Generic;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;
using Shuttle.Core.Threading;
using Shuttle.Core.Transactions;

namespace Shuttle.Esb
{
    public class ServiceBus : IServiceBus
    {
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
        private readonly IServiceBusConfiguration _configuration;
        private readonly IMessageSender _messageSender;
        private readonly IPipelineFactory _pipelineFactory;

        private IProcessorThreadPool _controlThreadPool;
        private IProcessorThreadPool _deferredMessageThreadPool;
        private IProcessorThreadPool _inboxThreadPool;
        private IProcessorThreadPool _outboxThreadPool;

        public ServiceBus(IServiceBusConfiguration configuration, ITransportMessageFactory transportMessageFactory,
            IPipelineFactory pipelineFactory, ISubscriptionManager subscriptionManager)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(transportMessageFactory, nameof(transportMessageFactory));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(subscriptionManager, nameof(subscriptionManager));

            _configuration = configuration;
            _pipelineFactory = pipelineFactory;

            _messageSender = new MessageSender(transportMessageFactory, _pipelineFactory, subscriptionManager);
        }

        public IServiceBus Start()
        {
            if (Started)
            {
                throw new ApplicationException(Resources.ServiceBusInstanceAlreadyStarted);
            }

            ConfigurationInvariant();

            var startupPipeline = _pipelineFactory.GetPipeline<StartupPipeline>();

            Started = true; // required for using ServiceBus in OnStarted event

            try
            {
                startupPipeline.Execute();

                _inboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("InboxThreadPool");
                _controlThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("ControlInboxThreadPool");
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

        public void Stop()
        {
            if (!Started)
            {
                return;
            }

            if (_configuration.HasInbox)
            {
                if (_configuration.Inbox.HasDeferredQueue)
                {
                    _deferredMessageThreadPool.Dispose();
                }

                _inboxThreadPool.Dispose();
            }

            if (_configuration.HasControlInbox)
            {
                _controlThreadPool.Dispose();
            }

            if (_configuration.HasOutbox)
            {
                _outboxThreadPool.Dispose();
            }

            _pipelineFactory.GetPipeline<ShutdownPipeline>().Execute();

            Started = false;
        }

        public bool Started { get; private set; }

        public void Dispose()
        {
            Stop();
        }

        public void Dispatch(TransportMessage transportMessage)
        {
            StartedGuard();

            _messageSender.Dispatch(transportMessage);
        }

        public TransportMessage Send(object message)
        {
            StartedGuard();

            return _messageSender.Send(message);
        }

        public TransportMessage Send(object message, Action<TransportMessageConfigurator> configure)
        {
            StartedGuard();

            return _messageSender.Send(message, configure);
        }

        public IEnumerable<TransportMessage> Publish(object message)
        {
            StartedGuard();

            return _messageSender.Publish(message);
        }

        public IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageConfigurator> configure)
        {
            StartedGuard();

            return _messageSender.Publish(message, configure);
        }

        private void StartedGuard()
        {
            if (Started)
            {
                return;
            }

            throw new InvalidOperationException(Resources.ServiceBusInstanceNotStarted);
        }

        private void ConfigurationInvariant()
        {
            Guard.Against<WorkerException>(_configuration.IsWorker && !_configuration.HasInbox,
                Resources.WorkerRequiresInboxException);

            if (_configuration.HasInbox)
            {
                Guard.Against<EsbConfigurationException>(
                    _configuration.Inbox.WorkQueue == null && string.IsNullOrEmpty(_configuration.Inbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "Inbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(
                    _configuration.Inbox.ErrorQueue == null && string.IsNullOrEmpty(_configuration.Inbox.ErrorQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "Inbox.ErrorQueueUri"));
            }

            if (_configuration.HasOutbox)
            {
                Guard.Against<EsbConfigurationException>(
                    _configuration.Outbox.WorkQueue == null && string.IsNullOrEmpty(_configuration.Outbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "Outbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(
                    _configuration.Outbox.ErrorQueue == null &&
                    string.IsNullOrEmpty(_configuration.Outbox.ErrorQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "Outbox.ErrorQueueUri"));
            }

            if (_configuration.HasControlInbox)
            {
                Guard.Against<EsbConfigurationException>(
                    _configuration.ControlInbox.WorkQueue == null &&
                    string.IsNullOrEmpty(_configuration.ControlInbox.WorkQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "ControlInbox.WorkQueueUri"));

                Guard.Against<EsbConfigurationException>(
                    _configuration.ControlInbox.ErrorQueue == null &&
                    string.IsNullOrEmpty(_configuration.ControlInbox.ErrorQueueUri),
                    string.Format(Resources.RequiredQueueUriMissing, "ControlInbox.ErrorQueueUri"));
            }
        }

        public static IServiceBusConfiguration Register(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, nameof(registry));

            var configuration = new ServiceBusConfiguration();

            new CoreConfigurator().Apply(configuration);
            new UriResolverConfigurator().Apply(configuration);
            new QueueManagerConfigurator().Apply(configuration);
            new MessageRouteConfigurator().Apply(configuration);
            new ControlInboxConfigurator().Apply(configuration);
            new InboxConfigurator().Apply(configuration);
            new OutboxConfigurator().Apply(configuration);
            new WorkerConfigurator().Apply(configuration);

            Register(registry, configuration);

            return configuration;
        }

        public static void Register(IComponentRegistry registry, IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(registry, nameof(registry));
            Guard.AgainstNull(configuration, nameof(configuration));

            registry.RegistryBoostrap();

            registry.AttemptRegister(configuration);

            registry.AttemptRegister<IServiceBusEvents, ServiceBusEvents>();
            registry.AttemptRegister<ISerializer, DefaultSerializer>();
            registry.AttemptRegister<IServiceBusPolicy, DefaultServiceBusPolicy>();
            registry.AttemptRegister<IMessageRouteProvider, DefaultMessageRouteProvider>();
            registry.AttemptRegister<IIdentityProvider, DefaultIdentityProvider>();
            registry.AttemptRegister<IMessageHandlerInvoker, DefaultMessageHandlerInvoker>();
            registry.AttemptRegister<IMessageHandlingAssessor, DefaultMessageHandlingAssessor>();
            registry.AttemptRegister<IUriResolver, DefaultUriResolver>();
            registry.AttemptRegister<IQueueManager, QueueManager>();
            registry.AttemptRegister<IWorkerAvailabilityManager, WorkerAvailabilityManager>();
            registry.AttemptRegister<ISubscriptionManager, NullSubscriptionManager>();
            registry.AttemptRegister<IIdempotenceService, NullIdempotenceService>();

            if (!registry.IsRegistered<ITransactionScopeFactory>())
            {
                var transactionScopeConfiguration = configuration.TransactionScope ??
                                                    new TransactionScopeConfiguration();

                registry.AttemptRegister<ITransactionScopeFactory>(
                    new DefaultTransactionScopeFactory(transactionScopeConfiguration.Enabled,
                        transactionScopeConfiguration.IsolationLevel,
                        TimeSpan.FromSeconds(transactionScopeConfiguration.TimeoutSeconds)));
            }

            registry.AttemptRegister<IPipelineFactory, DefaultPipelineFactory>();
            registry.AttemptRegister<ITransportMessageFactory, DefaultTransportMessageFactory>();

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypes<IPipeline>(typeof(ServiceBus).Assembly))
            {
                if (type.IsInterface || type.IsAbstract || registry.IsRegistered(type))
                {
                    continue;
                }

                registry.Register(type, type, Lifestyle.Transient);
            }

            var observers = new List<Type>();

            foreach (var type in reflectionService.GetTypes<IPipelineObserver>())
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                var interfaceType = type.InterfaceMatching($"I{type.Name}");

                if (interfaceType != null)
                {
                    if (registry.IsRegistered(type))
                    {
                        continue;
                    }

                    registry.Register(interfaceType, type, Lifestyle.Singleton);
                }
                else
                {
                    throw new EsbConfigurationException(string.Format(Resources.ObserverInterfaceMissingException, type.Name));
                }

                observers.Add(type);
            }

            registry.RegisterCollection(typeof(IPipelineObserver), observers, Lifestyle.Singleton);

            if (configuration.RegisterHandlers)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in reflectionService.GetTypes(MessageHandlerType, assembly))
                foreach (var @interface in type.GetInterfaces())
                {
                    if (!@interface.IsAssignableTo(MessageHandlerType))
                    {
                        continue;
                    }

                    var genericType = MessageHandlerType.MakeGenericType(@interface.GetGenericArguments()[0]);

                    if (!registry.IsRegistered(genericType))
                    {
                        registry.Register(genericType, type, Lifestyle.Transient);
                    }
                }
            }

            var queueFactoryType = typeof(IQueueFactory);
            var queueFactoryImplementationTypes = new List<Type>();

            void AddQueueFactoryImplementationType(Type type)
            {
                if (queueFactoryImplementationTypes.Contains(type))
                {
                    return;
                }

                queueFactoryImplementationTypes.Add(type);
            }

            if (configuration.ScanForQueueFactories)
            {
                foreach (var type in new ReflectionService().GetTypes<IQueueFactory>())
                {
                    AddQueueFactoryImplementationType(type);
                }
            }

            foreach (var type in configuration.QueueFactoryTypes)
            {
                AddQueueFactoryImplementationType(type);
            }

            registry.RegisterCollection(queueFactoryType, queueFactoryImplementationTypes, Lifestyle.Singleton);

            registry.AttemptRegister<IServiceBus, ServiceBus>();
        }

        public static IServiceBus Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            resolver.ResolverBoostrap();

            var configuration = resolver.Resolve<IServiceBusConfiguration>();

            if (configuration == null)
            {
                throw new InvalidOperationException(string.Format(Core.Container.Resources.TypeNotRegisteredException,
                    typeof(IServiceBusConfiguration).FullName));
            }

            configuration.Assign(resolver);

            var defaultPipelineFactory = resolver.Resolve<IPipelineFactory>() as DefaultPipelineFactory;

            if (defaultPipelineFactory != null)
            {
                defaultPipelineFactory.Assign(resolver);
            }

            return resolver.Resolve<IServiceBus>();
        }
    }
}