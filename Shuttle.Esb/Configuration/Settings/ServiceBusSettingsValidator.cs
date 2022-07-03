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
                if (string.IsNullOrWhiteSpace(settings.Inbox.WorkQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing,
                        "Inbox.WorkQueueUri"));
                }

                if (string.IsNullOrWhiteSpace(settings.Inbox.ErrorQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing,
                        "Inbox.ErrorQueueUri"));
                }
            }

            if (settings.Outbox != null)
            {
                if (string.IsNullOrWhiteSpace(settings.Outbox.WorkQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "Outbox.WorkQueueUri"));
                }

                if (string.IsNullOrWhiteSpace(settings.Outbox.ErrorQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "Outbox.ErrorQueueUri"));
                }
            }

            if (settings.ControlInbox != null)
            {
                if (string.IsNullOrWhiteSpace(settings.ControlInbox.WorkQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "ControlInbox.WorkQueueUri"));
                }

                if (string.IsNullOrWhiteSpace(settings.ControlInbox.ErrorQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "ControlInbox.ErrorQueueUri"));
                }
            }

            if (settings.QueueFactories != null)
            {
                foreach (var type in settings.QueueFactories.Types ?? Enumerable.Empty<string>())
                {
                    if (reflectionService.GetType(type) != null)
                    {
                        continue;
                    }

                    return ValidateOptionsResult.Fail(string.Format(Resources.UnknownTypeException, type));
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}