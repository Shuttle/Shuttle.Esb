//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using Shuttle.Core.Container;
//using Shuttle.Core.Contract;
//using Shuttle.Core.Pipelines;
//using Shuttle.Core.PipelineTransaction;
//using Shuttle.Core.Reflection;
//using Shuttle.Core.Serialization;
//using Shuttle.Core.System;
//using Shuttle.Core.Threading;
//using Shuttle.Core.Transactions;

//namespace Shuttle.Esb.Configuration
//{
//    public static class ComponentRegistryExtensions
//    {
//        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);

//        public static IServiceBusConfiguration RegisterServiceBus(this IComponentRegistry registry, IServiceBusConfiguration configuration = null)
//        {
//            Guard.AgainstNull(registry, nameof(registry));

//            var serviceBusConfiguration = configuration ?? new ServiceBusConfiguration();

//            if (configuration == null)
//            {
//                new CoreConfigurator().Apply(serviceBusConfiguration);
//                new UriResolverConfigurator().Apply(serviceBusConfiguration);
//                new QueueManagerConfigurator().Apply(serviceBusConfiguration);
//                new MessageRouteConfigurator().Apply(serviceBusConfiguration);
//                new ControlInboxConfigurator().Apply(serviceBusConfiguration);
//                new InboxConfigurator().Apply(serviceBusConfiguration);
//                new OutboxConfigurator().Apply(serviceBusConfiguration);
//                new WorkerConfigurator().Apply(serviceBusConfiguration);
//            }

//            registry.AttemptRegisterInstance(serviceBusConfiguration);

//            registry.AttemptRegister<IEnvironmentService, EnvironmentService>();
//            registry.AttemptRegister<IProcessService, ProcessService>();
//            registry.AttemptRegister<IServiceBusEvents, ServiceBusEvents>();
//            registry.AttemptRegister<ISerializer, DefaultSerializer>();
//            registry.AttemptRegister<IServiceBusPolicy, DefaultServiceBusPolicy>();
//            registry.AttemptRegister<IMessageRouteProvider, DefaultMessageRouteProvider>();
//            registry.AttemptRegister<IIdentityProvider, DefaultIdentityProvider>();
//            registry.AttemptRegister<IMessageHandlerInvoker, DefaultMessageHandlerInvoker>();
//            registry.AttemptRegister<IMessageHandlingAssessor, DefaultMessageHandlingAssessor>();
//            registry.AttemptRegister<IUriResolver, DefaultUriResolver>();
//            registry.AttemptRegister<IQueueManager, QueueManager>();
//            registry.AttemptRegister<IWorkerAvailabilityManager, WorkerAvailabilityManager>();
//            registry.AttemptRegister<ISubscriptionManager, NullSubscriptionManager>();
//            registry.AttemptRegister<IIdempotenceService, NullIdempotenceService>();
//            registry.AttemptRegister<ITransactionScopeObserver, TransactionScopeObserver>();
//            registry.AttemptRegister<ICancellationTokenSource, DefaultCancellationTokenSource>();

//            if (!registry.IsRegistered<ITransactionScopeFactory>())
//            {
//                var transactionScopeConfiguration = serviceBusConfiguration.TransactionScope ??
//                                                    new TransactionScopeConfiguration();

//                registry.AttemptRegisterInstance<ITransactionScopeFactory>(
//                    new DefaultTransactionScopeFactory(transactionScopeConfiguration.Enabled,
//                        transactionScopeConfiguration.IsolationLevel,
//                        TimeSpan.FromSeconds(transactionScopeConfiguration.TimeoutSeconds)));
//            }

//            registry.AttemptRegister<IPipelineFactory, DefaultPipelineFactory>();
//            registry.AttemptRegister<ITransportMessageFactory, TransportMessageFactory>();

//            var reflectionService = new ReflectionService();

//            foreach (var type in reflectionService.GetTypesAssignableTo<IPipeline>(typeof(ServiceBus).Assembly))
//            {
//                if (type.IsInterface || type.IsAbstract || registry.IsRegistered(type))
//                {
//                    continue;
//                }

//                registry.Register(type, type, Lifestyle.Transient);
//            }

//            foreach (var type in reflectionService.GetTypesAssignableTo<IPipelineObserver>(typeof(ServiceBus).Assembly))
//            {
//                if (type.IsInterface || type.IsAbstract)
//                {
//                    continue;
//                }

//                var interfaceType = type.InterfaceMatching($"I{type.Name}");

//                if (interfaceType != null)
//                {
//                    if (registry.IsRegistered(interfaceType))
//                    {
//                        continue;
//                    }

//                    registry.Register(interfaceType, type, Lifestyle.Singleton);
//                }
//                else
//                {
//                    throw new EsbConfigurationException(string.Format(Resources.ObserverInterfaceMissingException,
//                        type.Name));
//                }
//            }

//            if (serviceBusConfiguration.RegisterHandlers)
//            {
//                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
//                {
//                    registry.RegisterMessageHandlers(assembly);
//                }
//            }

//            var queueFactoryType = typeof(IQueueFactory);
//            var queueFactoryImplementationTypes = new HashSet<Type>();

//            void AddQueueFactoryImplementationType(Type type)
//            {
//                queueFactoryImplementationTypes.Add(type);
//            }

//            if (serviceBusConfiguration.ScanForQueueFactories)
//            {
//                foreach (var type in new ReflectionService().GetTypesAssignableTo<IQueueFactory>())
//                {
//                    AddQueueFactoryImplementationType(type);
//                }
//            }

//            foreach (var type in serviceBusConfiguration.QueueFactoryTypes)
//            {
//                AddQueueFactoryImplementationType(type);
//            }

//            registry.RegisterCollection(queueFactoryType, queueFactoryImplementationTypes, Lifestyle.Singleton);

//            registry.AttemptRegister<IServiceBus, ServiceBus>();

//            return serviceBusConfiguration;
//        }

//        public static void RegisterMessageHandlers(this IComponentRegistry registry, Assembly assembly)
//        {
//            var reflectionService = new ReflectionService();

//            foreach (var type in reflectionService.GetTypesAssignableTo(MessageHandlerType, assembly))
//                foreach (var @interface in type.GetInterfaces())
//                {
//                    if (!@interface.IsAssignableTo(MessageHandlerType))
//                    {
//                        continue;
//                    }

//                    var genericType = MessageHandlerType.MakeGenericType(@interface.GetGenericArguments()[0]);

//                    if (!registry.IsRegistered(genericType))
//                    {
//                        registry.Register(genericType, type, Lifestyle.Transient);
//                    }
//                }
//        }
//    }
//}