using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class OutboxSettingsValidator : IValidateOptions<OutboxSettings>
    {
        public ValidateOptionsResult Validate(string name, OutboxSettings options)
        {
            Guard.AgainstNull(options, nameof(options));

            if (string.IsNullOrWhiteSpace(options.WorkQueueUri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "Outbox.WorkQueueUri"));
            }

            if (string.IsNullOrWhiteSpace(options.ErrorQueueUri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredQueueUriMissing, "Outbox.ErrorQueueUri"));
            }

            return ValidateOptionsResult.Success;
        }
    }
}