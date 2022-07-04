using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class ServiceBusConfigurationBuilder
    {
        public const string SectionName = "Shuttle:ServiceBus";

        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);

        private readonly ServiceBusConfiguration _configuration = new ServiceBusConfiguration();
        private readonly ReflectionService _reflectionService = new ReflectionService();
        private readonly IServiceCollection _services;

        public ServiceBusConfigurationBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            services.PostConfigure<ServiceBusSettings>(settings =>
            {
                if (settings.Inbox != null)
                {
                    _configuration.Inbox =
                        new InboxQueueConfiguration
                        {
                            WorkQueueUri = settings.Inbox.WorkQueueUri,
                            DeferredQueueUri = settings.Inbox.DeferredQueueUri,
                            ErrorQueueUri = settings.Inbox.ErrorQueueUri,
                            ThreadCount = settings.Inbox.ThreadCount,
                            MaximumFailureCount = settings.Inbox.MaximumFailureCount,
                            DurationToIgnoreOnFailure =
                                settings.Inbox.DurationToIgnoreOnFailure ??
                                ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
                            DurationToSleepWhenIdle =
                                settings.Inbox.DurationToSleepWhenIdle ??
                                ServiceBusConfiguration.DefaultDurationToSleepWhenIdle,
                            Distribute = settings.Inbox.Distribute,
                            DistributeSendCount = settings.Inbox.DistributeSendCount
                        };
                }

                if (settings.Outbox != null)
                {
                    _configuration.Outbox =
                        new OutboxQueueConfiguration
                        {
                            WorkQueueUri = settings.Outbox.WorkQueueUri,
                            ErrorQueueUri = settings.Outbox.ErrorQueueUri,
                            MaximumFailureCount = settings.Outbox.MaximumFailureCount,
                            DurationToIgnoreOnFailure =
                                settings.Outbox.DurationToIgnoreOnFailure ??
                                ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
                            DurationToSleepWhenIdle =
                                settings.Outbox.DurationToSleepWhenIdle ??
                                ServiceBusConfiguration.DefaultDurationToSleepWhenIdle,
                            ThreadCount = settings.Outbox.ThreadCount
                        };
                }

                if (settings.Worker != null)
                {
                    _configuration.Worker = new WorkerConfiguration
                    {
                        DistributorControlInboxWorkQueueUri =
                            settings.Worker.DistributorControlWorkQueueUri,
                        ThreadAvailableNotificationIntervalSeconds =
                            settings.Worker.ThreadAvailableNotificationIntervalSeconds
                    };
                }

                if (settings.ControlInbox != null)
                {
                    _configuration.ControlInbox =
                        new ControlInboxQueueConfiguration
                        {
                            WorkQueueUri = settings.ControlInbox.WorkQueueUri,
                            ErrorQueueUri = settings.ControlInbox.ErrorQueueUri,
                            ThreadCount = settings.ControlInbox.ThreadCount,
                            MaximumFailureCount = settings.ControlInbox.MaximumFailureCount,
                            DurationToIgnoreOnFailure =
                                settings.ControlInbox.DurationToIgnoreOnFailure ??
                                ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
                            DurationToSleepWhenIdle =
                                settings.ControlInbox.DurationToSleepWhenIdle ??
                                ServiceBusConfiguration.DefaultDurationToSleepWhenIdle
                        };
                }
            });

            _services = services;
        }

        public ServiceBusConfigurationBuilder AddMessageHandlers(Assembly assembly)
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

                if (!_services.Contains(ServiceDescriptor.Transient(genericType, type)))
                {
                    _services.AddTransient(genericType, type);
                }
            }

            return this;
        }

        public ServiceBusConfigurationBuilder AddMessageRoute(MessageRouteConfiguration messageRoute)
        {
            Guard.AgainstNull(messageRoute, nameof(messageRoute));

            _configuration.AddMessageRoute(messageRoute);

            return this;
        }

        public ServiceBusConfigurationBuilder AddUriMapping(Uri sourceUri, Uri targetUri)
        {
            _configuration.AddUriMapping(sourceUri, targetUri);

            return this;
        }

        public ServiceBusConfigurationBuilder AddSettings(ServiceBusSettings settings)
        {
            Guard.AgainstNull(settings, nameof(settings));

            _services.AddOptions<ServiceBusSettings>().Configure(options =>
            {
                Apply(settings, options);
            });

            return this;
        }

        private static void Apply(ServiceBusSettings settings, ServiceBusSettings options)
        {
            options.ControlInbox = settings.ControlInbox;
            options.Inbox = settings.Inbox;
            options.CacheIdentity = settings.CacheIdentity;
            options.CompressionAlgorithm = settings.CompressionAlgorithm;
            options.CreateQueues = settings.CreateQueues;
            options.EncryptionAlgorithm = settings.EncryptionAlgorithm;
            options.MessageRoutes = settings.MessageRoutes;
            options.Outbox = settings.Outbox;
            options.RegisterHandlers = settings.RegisterHandlers;
            options.RemoveCorruptMessages = settings.RemoveCorruptMessages;
            options.RemoveMessagesNotHandled = settings.RemoveMessagesNotHandled;
        }

        public ServiceBusConfigurationBuilder AddSettings(string key = null)
        {
            _services.AddOptions<ServiceBusSettings>().Configure<IConfiguration>((options, configuration) =>
            {
                var sectionName = key ?? SectionName;

                var settings = configuration.GetRequiredSection(sectionName).Get<ServiceBusSettings>();

                if (settings == null)
                {
                    throw new EsbConfigurationException(string.Format(Resources.SectionKeyMissingException,
                        sectionName));
                }

                Apply(settings, options);
            });

            return this;
        }

        internal IServiceBusConfiguration GetConfiguration()
        {
            AddMessageHandlers(_configuration);

            return _configuration;
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

        public ServiceBusConfigurationBuilder Configure(Action<ServiceBusConfiguration> configure)
        {
            Guard.AgainstNull(configure, nameof(configure));

            configure.Invoke(_configuration);

            return this;
        }

        public ServiceBusConfigurationBuilder Configure(IServiceBusConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            AddMessageHandlers(configuration);

            _services.AddSingleton(configuration);

            return this;
        }
    }
}