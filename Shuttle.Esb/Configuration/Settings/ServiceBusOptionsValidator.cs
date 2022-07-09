using System.Linq;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Esb
{
    public class ServiceBusOptionsValidator : IValidateOptions<ServiceBusOptions>
    {
        public ValidateOptionsResult Validate(string name, ServiceBusOptions options)
        {
            Guard.AgainstNull(options, nameof(options));

            if (options.Inbox != null)
            {
                if (string.IsNullOrWhiteSpace(options.Inbox.WorkQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissingException, "Inbox.WorkQueueUri"));
                }
            }

            if (options.Outbox != null)
            {
                if (string.IsNullOrWhiteSpace(options.Outbox.WorkQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissingException, "Outbox.WorkQueueUri"));
                }
            }

            if (options.ControlInbox != null)
            {
                if (string.IsNullOrWhiteSpace(options.ControlInbox.WorkQueueUri))
                {
                    return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissingException, "ControlInbox.WorkQueueUri"));
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}