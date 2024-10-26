using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class ServiceBusOptionsValidator : IValidateOptions<ServiceBusOptions>
{
    public ValidateOptionsResult Validate(string? name, ServiceBusOptions options)
    {
        Guard.AgainstNull(options);

        if (options.Inbox != null)
        {
            if (string.IsNullOrWhiteSpace(options.Inbox!.WorkQueueUri))
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

        foreach (var messageRoute in options.MessageRoutes)
        {
            if (!Uri.TryCreate(messageRoute.Uri, UriKind.RelativeOrAbsolute, out _))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.InvalidUriException, messageRoute.Uri, "MessageRoute.Uri"));
            }

            if (!(messageRoute.Specifications).Any())
            {
                return ValidateOptionsResult.Fail(Resources.MessageRoutesRequireSpecificationException);
            }
        }

        foreach (var uriMapping in options.UriMappings)
        {
            if (!Uri.TryCreate(uriMapping.SourceUri, UriKind.RelativeOrAbsolute, out _))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.InvalidUriException, uriMapping.SourceUri, nameof(uriMapping.SourceUri)));
            }

            if (!Uri.TryCreate(uriMapping.TargetUri, UriKind.RelativeOrAbsolute, out _))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.InvalidUriException, uriMapping.TargetUri, nameof(uriMapping.TargetUri)));
            }
        }

        return ValidateOptionsResult.Success;
    }
}