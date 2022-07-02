using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class InboxSettingsValidator : IValidateOptions<InboxSettings>
    {
        public ValidateOptionsResult Validate(string name, InboxSettings options)
        {
            Guard.AgainstNull(options, nameof(options));

            if (string.IsNullOrWhiteSpace(options.WorkQueueUri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "Inbox.WorkQueueUri"));
            }

            if (string.IsNullOrWhiteSpace(options.ErrorQueueUri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "Inbox.ErrorQueueUri"));
            }

            return ValidateOptionsResult.Success;
        }
    }
}