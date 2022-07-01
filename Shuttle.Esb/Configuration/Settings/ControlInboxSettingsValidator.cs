using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class ControlInboxSettingsValidator : IValidateOptions<ControlInboxSettings>
    {
        public ValidateOptionsResult Validate(string name, ControlInboxSettings options)
        {
            Guard.AgainstNull(options, nameof(options));

            if (string.IsNullOrWhiteSpace(options.Uri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Control.WorkUri"));
            }

            if (string.IsNullOrWhiteSpace(options.ErrorUri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Control.ErrorUri"));
            }

            return ValidateOptionsResult.Success;
        }
    }
}