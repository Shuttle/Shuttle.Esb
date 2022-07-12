using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class ServiceBusBuilder
    {
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);

        public ServiceBusConfiguration Configuration
        {
            get => _serviceBusConfiguration;
            set => _serviceBusConfiguration = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ServiceBusOptions Options
        {
            get => _serviceBusOptions;
            set => _serviceBusOptions = value ?? throw new ArgumentNullException(nameof(value));
        }


        private readonly ReflectionService _reflectionService = new ReflectionService();
        private ServiceBusConfiguration _serviceBusConfiguration = new ServiceBusConfiguration();
        private ServiceBusOptions _serviceBusOptions = new ServiceBusOptions();

        public ServiceBusBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public IServiceCollection Services { get; }

        public ServiceBusBuilder AddMessageHandlers(Assembly assembly)
        {
            Guard.AgainstNull(assembly, nameof(assembly));

            foreach (var type in _reflectionService.GetTypesAssignableTo(MessageHandlerType, assembly))
            foreach (var @interface in type.GetInterfaces())
            {
                if (!@interface.IsAssignableTo(MessageHandlerType))
                {
                    continue;
                }

                var genericType = MessageHandlerType.MakeGenericType(@interface.GetGenericArguments()[0]);

                if (!Services.Contains(ServiceDescriptor.Transient(genericType, type)))
                {
                    Services.AddTransient(genericType, type);
                }
            }

            return this;
        }

        internal IServiceBusConfiguration GetConfiguration()
        {
            AddMessageHandlers();

            return Configuration;
        }

        private void AddMessageHandlers()
        {
            if (Options.AddMessageHandlers)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    AddMessageHandlers(assembly);
                }
            }
            else
            {
                AddMessageHandlers(typeof(ServiceBus).Assembly);
            }
        }

        public ServiceBusBuilder AddModule<T>() where T : class
        {
            AddModule(typeof(T));

            return this;
        }

        public ServiceBusBuilder AddModule(Type type) 
        {
            Guard.AgainstNull(type, nameof(type));

            Configuration.AddModule(type);

            Services.TryAddSingleton(type, type);

            return this;
        }
    }
}