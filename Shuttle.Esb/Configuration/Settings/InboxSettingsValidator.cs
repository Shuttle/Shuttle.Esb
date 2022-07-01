using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class InboxSettingsValidator : IValidateOptions<InboxSettings>
    {
        public ValidateOptionsResult Validate(string name, InboxSettings options)
        {
            Guard.AgainstNull(options, nameof(options));

            if (string.IsNullOrWhiteSpace(options.Uri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Inbox.WorkUri"));
            }

            if (string.IsNullOrWhiteSpace(options.ErrorUri))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.RequiredBrokerEndpointUriMissing, "Inbox.ErrorUri"));
            }

            return ValidateOptionsResult.Success;
        }
    }
}