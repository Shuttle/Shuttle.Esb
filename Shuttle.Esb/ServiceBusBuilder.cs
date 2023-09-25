using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class ServiceBusBuilder
    {
        private static readonly Type MessageHandlerType = typeof(IAsyncMessageHandler<>);

        public ServiceBusOptions Options
        {
            get => _serviceBusOptions;
            set => _serviceBusOptions = value ?? throw new ArgumentNullException(nameof(value));
        }
        
        private readonly ReflectionService _reflectionService = new ReflectionService();
        private ServiceBusOptions _serviceBusOptions = new ServiceBusOptions();

        public ServiceBusBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public IServiceCollection Services { get; }
        
        public bool SuppressHostedService { get; set; }

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

        public ServiceBusBuilder AddSubscription<T>()
        {
            AddSubscription(typeof(T));

            return this;
        }

        public ServiceBusBuilder AddSubscription(Type messageType)
        {
            Guard.AgainstNull(messageType, nameof(messageType));

            AddSubscription(messageType.FullName);

            return this;
        }

        public ServiceBusBuilder AddSubscription(string messageType)
        {
            Guard.AgainstNullOrEmptyString(messageType, nameof(messageType));

            var messageTypes = _serviceBusOptions?.Subscription?.MessageTypes;

            if (messageTypes == null)
            {
                throw new InvalidOperationException(Resources.AddSubscriptionException);
            }

            if (!messageTypes.Contains(messageType))
            {
                messageTypes.Add(messageType);
            }

            return this;
        }
    }
}