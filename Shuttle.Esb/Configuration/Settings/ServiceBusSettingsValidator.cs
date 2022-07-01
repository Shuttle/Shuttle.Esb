using System.Linq;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class ServiceBusSettingsValidator : IValidateOptions<ServiceBusSettings>
    {
        public ValidateOptionsResult Validate(string name, ServiceBusSettings settings)
        {
            Guard.AgainstNull(settings, nameof(settings));

            var reflectionService = new ReflectionService();

            if (settings.Inbox != null)
            {
                if (string.IsNullOrWhiteSpace(settings.Inbox.Uri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing,
                        "Inbox.Uri"));
                }

                if (string.IsNullOrWhiteSpace(settings.Inbox.ErrorUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing,
                        "Inbox.ErrorUri"));
                }
            }

            if (settings.Outbox != null)
            {
                if (string.IsNullOrWhiteSpace(settings.Outbox.Uri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Outbox.Uri"));
                }

                if (string.IsNullOrWhiteSpace(settings.Outbox.ErrorUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Outbox.ErrorUri"));
                }
            }

            if (settings.ControlInbox != null)
            {
                if (string.IsNullOrWhiteSpace(settings.ControlInbox.Uri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Control.Uri"));
                }

                if (string.IsNullOrWhiteSpace(settings.ControlInbox.ErrorUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Control.ErrorUri"));
                }
            }

            if (settings.BrokerEndpointsFactories != null)
            {
                foreach (var type in settings.BrokerEndpointsFactories.Types ?? Enumerable.Empty<string>())
                {
                    if (reflectionService.GetType(type) != null)
                    {
                        continue;
                    }

                    return ValidateOptionsResult.Fail(string.Format(Resources.UnknownTypeException, type));
                }
            }

            if (settings.Worker != null && settings.Inbox == null)
            {
                return ValidateOptionsResult.Fail(Resources.WorkerRequiresInboxException);
            }

            return ValidateOptionsResult.Success;
        }
    }
}