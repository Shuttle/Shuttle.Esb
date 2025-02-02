using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class QueueUri
{
    public QueueUri(Uri uri)
    {
        Uri = Guard.AgainstNull(uri);

        if (uri.LocalPath == "/" || uri.Segments.Length != 2)
        {
            throw new UriFormatException(string.Format(Resources.UriFormatException, $"{uri.Scheme}://{{configuration-name}}/{{topic}}", uri));
        }

        ConfigurationName = Uri.Host;
        QueueName = Uri.Segments[1];
    }

    public QueueUri(string uri) : this(new Uri(uri))
    {
    }

    public string ConfigurationName { get; }

    public string QueueName { get; }
    public Uri Uri { get; }

    public QueueUri SchemeInvariant(string scheme)
    {
        if (!Uri.Scheme.Equals(Guard.AgainstNullOrEmptyString(scheme), StringComparison.InvariantCultureIgnoreCase))
        {
            throw new InvalidSchemeException(Uri.Scheme, Uri.ToString());
        }

        return this;
    }

    public override string ToString()
    {
        return Uri.ToString();
    }
}