using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ControlInboxSettingsValidator : IValidateOptions<ControlInboxSettings>
    {
        public ValidateOptionsResult Validate(string name, ControlInboxSettings options)
        {
            Guard.AgainstNull(options, nameof(options));

            if (string.IsNullOrWhiteSpace(options.WorkQueueUri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "ControlInbox.WorkQueueUri"));
            }

            if (string.IsNullOrWhiteSpace(options.ErrorQueueUri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "ControlInbox.ErrorQueueUri"));
            }

            return ValidateOptionsResult.Success;
        }
    }
}