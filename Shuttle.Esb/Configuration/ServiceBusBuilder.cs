using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class ServiceBusBuilder
    {
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);

        public ServiceBusConfiguration Configuration
        {
            get => _configuration;
            set => _configuration = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ServiceBusOptions Options
        {
            get => _options;
            set => _options = value ?? throw new ArgumentNullException(nameof(value));
        }


        private readonly ReflectionService _reflectionService = new ReflectionService();
        private ServiceBusConfiguration _configuration = new ServiceBusConfiguration();
        private ServiceBusOptions _options = new ServiceBusOptions();

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

        public ServiceBusBuilder AddMessageRoute(MessageRouteConfiguration messageRoute)
        {
            Guard.AgainstNull(messageRoute, nameof(messageRoute));

            Configuration.AddMessageRoute(messageRoute);

            return this;
        }

        public ServiceBusBuilder AddUriMapping(Uri sourceUri, Uri targetUri)
        {
            Configuration.AddUriMapping(sourceUri, targetUri);

            return this;
        }

        internal IServiceBusConfiguration GetConfiguration()
        {
            AddMessageHandlers(Configuration);

            return Configuration;
        }

        private void AddMessageHandlers(IServiceBusConfiguration configuration)
        {
            if (configuration.ShouldAddMessageHandlers)
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
    }
}