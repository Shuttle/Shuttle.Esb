using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb;

public class MessageHandlerDelegate
{
    private readonly Type _handlerContextType = typeof(IHandlerContext);
    private readonly IEnumerable<Type> _parameterTypes;

    public MessageHandlerDelegate(Delegate handler, IEnumerable<Type> parameterTypes)
    {
        Handler = handler;
        HasParameters = parameterTypes.Any();
        _parameterTypes = parameterTypes;
    }

    public Delegate Handler { get; }
    public bool HasParameters { get; }

    public object[] GetParameters(IServiceProvider serviceProvider, object handlerContext)
    {
        return _parameterTypes
            .Select(parameterType => !parameterType.IsCastableTo(_handlerContextType)
                ? serviceProvider.GetRequiredService(parameterType)
                : handlerContext
            ).ToArray();
    }
}