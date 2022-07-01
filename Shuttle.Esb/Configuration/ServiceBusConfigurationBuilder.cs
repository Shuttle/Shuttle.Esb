using System;
using System.Collections.Generic;
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
        private static readonly Type BrokerEndpointFactoryType = typeof(IBrokerEndpointFactory);

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
                        new InboxConfiguration
                        {
                            Uri = settings.Inbox.Uri,
                            DeferredUri = settings.Inbox.DeferredUri,
                            ErrorUri = settings.Inbox.ErrorUri,
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
                        new OutboxConfiguration
                        {
                            Uri = settings.Outbox.Uri,
                            ErrorUri = settings.Outbox.ErrorUri,
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
                        DistributorControlUri =
                            settings.Worker.DistributorControlUri,
                        ThreadAvailableNotificationIntervalSeconds =
                            settings.Worker.ThreadAvailableNotificationIntervalSeconds
                    };
                }

                if (settings.ControlInbox != null)
                {
                    _configuration.Control =
                        new ControlConfiguration
                        {
                            Uri = settings.ControlInbox.Uri,
                            ErrorUri = settings.ControlInbox.ErrorUri,
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

                if (settings.BrokerEndpointsFactories != null)
                {
                    var added = new List<Type>();

                    _configuration.ScanForBrokerEndpointFactories = settings.BrokerEndpointsFactories.Scan;

                    var types = (from type in settings.BrokerEndpointsFactories.Types ?? Enumerable.Empty<string>()
                        select _reflectionService.GetType(type)).ToList();

                    if (_configuration.ScanForBrokerEndpointFactories)
                    {
                        types.AddRange(_reflectionService.GetTypesAssignableTo<IBrokerEndpointFactory>().ToList());
                    }

                    foreach (var type in types)
                    {
                        if (added.Contains(type))
                        {
                            continue;
                        }

                        _services.AddSingleton(BrokerEndpointFactoryType, type);

                        added.Add(type);
                    }
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
            options.CreateBrokerEndpoints = settings.CreateBrokerEndpoints;
            options.EncryptionAlgorithm = settings.EncryptionAlgorithm;
            options.MessageRoutes = settings.MessageRoutes;
            options.Outbox = settings.Outbox;
            options.RegisterHandlers = settings.RegisterHandlers;
            options.RemoveCorruptMessages = settings.RemoveCorruptMessages;
            options.RemoveMessagesNotHandled = settings.RemoveMessagesNotHandled;
        }

        public ServiceBusConfigurationBuilder ReadSettings(string key = null)
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

        public ServiceBusConfigurationBuilder ReadConfiguration()
        {
            _services.AddOptions<ServiceBusSettings>().Configure(settings =>
            {
                var section = ServiceBusSection.Get();

                if (section == null)
                {
                    return;
                }

                settings.CreateBrokerEndpoints = section.CreateBrokerEndpoints;
                settings.CacheIdentity = section.CacheIdentity;
                settings.RegisterHandlers = section.RegisterHandlers;
                settings.RemoveMessagesNotHandled = section.RemoveMessagesNotHandled;
                settings.RemoveCorruptMessages = section.RemoveCorruptMessages;
                settings.CompressionAlgorithm = section.CompressionAlgorithm;
                settings.EncryptionAlgorithm = section.EncryptionAlgorithm;

                if (section.Inbox != null)
                {
                    settings.Inbox = new InboxSettings
                    {
                        Uri = section.Inbox.Uri,
                        DeferredUri = section.Inbox.DeferredUri,
                        ErrorUri = section.Inbox.ErrorUri,
                        ThreadCount = section.Inbox.ThreadCount,
                        MaximumFailureCount = section.Inbox.MaximumFailureCount,
                        DurationToIgnoreOnFailure =
                            section.Inbox.DurationToIgnoreOnFailure ??
                            ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
                        DurationToSleepWhenIdle =
                            section.Inbox.DurationToSleepWhenIdle ??
                            ServiceBusConfiguration.DefaultDurationToSleepWhenIdle,
                        Distribute = section.Inbox.Distribute,
                        DistributeSendCount = section.Inbox.DistributeSendCount
                    };
                }

                if (section.Control != null)
                {
                    settings.ControlInbox = new ControlInboxSettings
                    {
                        Uri = section.Control.Uri,
                        ErrorUri = section.Control.ErrorUri,
                        ThreadCount = section.Control.ThreadCount,
                        MaximumFailureCount = section.Control.MaximumFailureCount,
                        DurationToIgnoreOnFailure =
                            section.Control.DurationToIgnoreOnFailure ??
                            ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
                        DurationToSleepWhenIdle =
                            section.Control.DurationToSleepWhenIdle ??
                            ServiceBusConfiguration.DefaultDurationToSleepWhenIdle
                    };
                }

                if (section.Outbox != null)
                {
                    settings.Outbox =
                        new OutboxSettings
                        {
                            Uri = section.Outbox.Uri,
                            ErrorUri = section.Outbox.ErrorUri,
                            MaximumFailureCount = section.Outbox.MaximumFailureCount,
                            DurationToIgnoreOnFailure =
                                section.Outbox.DurationToIgnoreOnFailure ??
                                ServiceBusConfiguration.DefaultDurationToIgnoreOnFailure,
                            DurationToSleepWhenIdle =
                                section.Outbox.DurationToSleepWhenIdle ??
                                ServiceBusConfiguration.DefaultDurationToSleepWhenIdle,
                            ThreadCount = section.Outbox.ThreadCount
                        };
                }

                if (section.MessageRoutes != null)
                {
                    var messageRoutes = new List<MessageRouteSettings>();
                    
                    foreach (MessageRouteElement mapElement in ServiceBusSection.Get().MessageRoutes)
                    {
                        var specifications = new List<MessageRouteSettings.SpecificationSettings>();

                        foreach (SpecificationElement specificationElement in mapElement)
                        {
                            specifications.Add(new MessageRouteSettings.SpecificationSettings
                            {
                                Name = specificationElement.Name,
                                Value = specificationElement.Value
                            });
                        }

                        if (specifications.Any())
                        {
                            messageRoutes.Add(new MessageRouteSettings
                            {
                                Uri = mapElement.Uri,
                                Specifications = specifications.ToArray()
                            });
                        }
                    }

                    settings.MessageRoutes = messageRoutes.ToArray();
                }

                if (section.Worker != null)
                {
                    settings.Worker = new WorkerSettings
                    {
                        DistributorControlUri = section.Worker.DistributorControlUri,
                        ThreadAvailableNotificationIntervalSeconds =
                            section.Worker.ThreadAvailableNotificationIntervalSeconds
                    };
                }

                if (section.BrokerEndpointEndpointFactories != null)
                {
                    var types = new List<string>();

                    foreach (BrokerEndpointFactoryElement queueFactoryElement in ServiceBusSection.Get().BrokerEndpointEndpointFactories)
                    {
                        types.Add(queueFactoryElement.Type);
                    }


                    settings.BrokerEndpointsFactories = new BrokerEndpointsFactoriesSettings
                    {
                        Scan = section.BrokerEndpointEndpointFactories.Scan,
                        Types = types.ToArray()
                    };
                }
            });

            return this;
        }

        internal IServiceBusConfiguration GetConfiguration()
        {
            if (_configuration.RegisterHandlers)
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

            return _configuration;
        }

        public ServiceBusConfigurationBuilder Configure(Action<ServiceBusConfiguration> configure)
        {
            Guard.AgainstNull(configure, nameof(configure));

            configure.Invoke(_configuration);

            return this;
        }
    }
}