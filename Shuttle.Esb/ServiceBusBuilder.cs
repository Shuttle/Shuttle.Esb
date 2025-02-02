using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb;

public class ServiceBusBuilder
{
    public event EventHandler<AddPipelineProcessingEventArgs>? AddPipelineProcessing;

    private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);

    private readonly ReflectionService _reflectionService = new();
    private ServiceBusOptions _serviceBusOptions = new();
    private readonly Dictionary<Type, MessageHandlerDelegate> _delegates = new();

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

    public IDictionary<Type, MessageHandlerDelegate> GetDelegates() => new ReadOnlyDictionary<Type, MessageHandlerDelegate>(_delegates);

    public bool ShouldSuppressHostedService { get; private set; }
    public bool ShouldSuppressPipelineTransactionScope { get; private set; }
    public bool ShouldSuppressPipelineProcessing { get; private set; }

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

    public ServiceBusBuilder SuppressPipelineProcessing()
    {
        ShouldSuppressPipelineProcessing = true;

        return this;
    }

    public ServiceBusBuilder AddMessageHandler(Delegate handler)
    {
        if (!typeof(Task).IsAssignableFrom(Guard.AgainstNull(handler).Method.ReturnType))
        {
            throw new ApplicationException(Core.Pipelines.Resources.AsyncDelegateRequiredException);
        }

        var parameters = handler.Method.GetParameters();

        Type? messageType = null;

        foreach (var parameter in parameters)
        {
            var parameterType = parameter.ParameterType;

            if (parameterType.IsCastableTo(typeof(IHandlerContext<>)))
            {
                messageType = parameterType.GetGenericArguments()[0];
            }
        }

        if (messageType == null)
        {
            throw new ApplicationException(Resources.MessageHandlerTypeException);
        }

        if (!_delegates.TryAdd(messageType, new(handler, handler.Method.GetParameters().Select(item => item.ParameterType))))
        {
            throw new InvalidOperationException(string.Format(Resources.DelegateAlreadyRegisteredException, messageType.FullName));
        }

        return this;
    }

    public ServiceBusBuilder AddMessageHandlers(Assembly assembly, Func<Type, ServiceLifetime>? getServiceLifetime = null)
    {
        getServiceLifetime ??= _ => ServiceLifetime.Singleton;

        foreach (var type in _reflectionService.GetTypesCastableToAsync(MessageHandlerType, Guard.AgainstNull(assembly)).GetAwaiter().GetResult())
        foreach (var @interface in type.InterfacesCastableTo(MessageHandlerType))
        {
            var genericType = MessageHandlerType.MakeGenericType(@interface.GetGenericArguments()[0]);
            var serviceDescriptor = new ServiceDescriptor(genericType, type, getServiceLifetime(genericType));

            if (Services.Contains(serviceDescriptor))
            {
                throw new InvalidOperationException(string.Format(Resources.MessageHandlerAlreadyRegisteredException, type.FullName));
            }

            Services.Add(serviceDescriptor);
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

    public ServiceBusBuilder AddMessageHandler(object messageHandler)
    {
        var type = Guard.AgainstNull(messageHandler).GetType();

        foreach (var @interface in type.InterfacesCastableTo(MessageHandlerType))
        {
            var genericType = MessageHandlerType.MakeGenericType(@interface.GetGenericArguments()[0]);
            var serviceDescriptor = new ServiceDescriptor(genericType, type, ServiceLifetime.Singleton);

            if (Services.Contains(serviceDescriptor))
            {
                throw new InvalidOperationException(string.Format(Resources.MessageHandlerAlreadyRegisteredException, type.FullName));
            }

            Services.Add(serviceDescriptor);
        }

        return this;
    }

    public void OnAddPipelineProcessing(PipelineProcessingBuilder pipelineProcessingBuilder)
    {
        AddPipelineProcessing?.Invoke(this, new(pipelineProcessingBuilder));
    }
}