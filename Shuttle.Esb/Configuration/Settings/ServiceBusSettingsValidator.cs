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
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "Inbox.WorkQueueUri"));
                }
            }

            if (settings.Outbox != null)
            {
                if (string.IsNullOrWhiteSpace(settings.Outbox.WorkQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "Outbox.WorkQueueUri"));
                }
            }

            if (settings.ControlInbox != null)
            {
                if (string.IsNullOrWhiteSpace(settings.ControlInbox.WorkQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "ControlInbox.WorkQueueUri"));
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}