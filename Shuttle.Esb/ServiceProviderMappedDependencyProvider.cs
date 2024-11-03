using System;
using Microsoft.Extensions.DependencyInjection;

namespace Shuttle.Esb;

internal class ServiceProviderMappedDependencyProvider : IMappedDependencyProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _type;

    public ServiceProviderMappedDependencyProvider(IServiceProvider serviceProvider, Type type)
    {
        _serviceProvider = serviceProvider;
        _type = type;
    }

    public object Get()
    {
        return _serviceProvider.GetRequiredService(_type);
    }
}