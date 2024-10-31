using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb;

public class ServiceBusBuilder
{
    private static readonly Type AsyncMessageHandlerType = typeof(IMessageHandler<>);

    private readonly ReflectionService _reflectionService = new();
    private ServiceBusOptions _serviceBusOptions = new();

    public ServiceBusBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public ServiceBusOptions Options
    {
        get => _serviceBusOptions;
        set => _serviceBusOptions = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IServiceCollection Services { get; }

    public bool ShouldSuppressHostedService { get; private set; }
    public bool ShouldSuppressPipelineTransactionScope { get; private set; }

    public ServiceBusBuilder SuppressHostedService()
    {
        ShouldSuppressHostedService = true;

        return this;
    }

    public ServiceBusBuilder SuppressPipelineTransactionScope()
    {
        ShouldSuppressPipelineTransactionScope = true;

        return this;
    }

    public ServiceBusBuilder AddMessageHandlers(Assembly assembly)
    {
        foreach (var type in _reflectionService.GetTypesCastableToAsync(AsyncMessageHandlerType, Guard.AgainstNull(assembly)).GetAwaiter().GetResult())
        foreach (var @interface in type.GetInterfaces())
        {
            if (!@interface.IsCastableTo(AsyncMessageHandlerType))
            {
                continue;
            }

            var genericType = AsyncMessageHandlerType.MakeGenericType(@interface.GetGenericArguments()[0]);

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
        AddSubscription(Guard.AgainstNullOrEmptyString(Guard.AgainstNull(messageType).FullName));

        return this;
    }

    public ServiceBusBuilder AddSubscription(string messageType)
    {
        Guard.AgainstNullOrEmptyString(messageType);

        var messageTypes = _serviceBusOptions.Subscription.MessageTypes;

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