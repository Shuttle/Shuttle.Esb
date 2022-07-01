using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class OutboxSettingsValidator : IValidateOptions<OutboxSettings>
    {
        public ValidateOptionsResult Validate(string name, OutboxSettings options)
        {
            Guard.AgainstNull(options, nameof(options));

            if (string.IsNullOrWhiteSpace(options.Uri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Outbox.Uri"));
            }

            if (string.IsNullOrWhiteSpace(options.ErrorUri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Outbox.ErrorUri"));
            }

            return ValidateOptionsResult.Success;
        }
    }
}