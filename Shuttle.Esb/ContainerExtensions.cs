using System;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public static class ContainerExtensions
    {
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);

        public static void RegisterMessageHandlers(this IComponentContainer container)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                RegisterMessageHandlers(container, assembly);
            }
        }

        public static void RegisterMessageHandlers(this IComponentContainer container, Assembly assembly)
        {
            Guard.AgainstNull(container, "container");

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypes(MessageHandlerType, assembly))
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    if (!@interface.IsAssignableTo(MessageHandlerType))
                    {
                        continue;
                    }

                    container.Register(MessageHandlerType.MakeGenericType(@interface.GetGenericArguments()[0]), type, Lifestyle.Thread);
                }
            }

        }
    }
}