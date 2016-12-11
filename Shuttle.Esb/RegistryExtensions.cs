using System;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public static class RegistryExtensions
    {
        private static readonly Type MessageHandlerType = typeof (IMessageHandler<>);

        public static void RegisterMessageHandlers(this IComponentRegistry registry)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                RegisterMessageHandlers(registry, assembly);
            }
        }

        public static void RegisterMessageHandlers(this IComponentRegistry registry, Assembly assembly)
        {
            Guard.AgainstNull(registry, "registry");

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypes(MessageHandlerType, assembly))
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    if (!@interface.IsAssignableTo(MessageHandlerType))
                    {
                        continue;
                    }

                    registry.Register(MessageHandlerType.MakeGenericType(@interface.GetGenericArguments()[0]), type,
                        Lifestyle.Transient);
                }
            }
        }
    }
}