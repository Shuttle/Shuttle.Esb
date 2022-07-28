using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class QueueUri
    {
        public Uri Uri { get; }

        public QueueUri(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));

            Uri = uri;

            if (uri.LocalPath == "/" || uri.Segments.Length != 2)
            {
                throw new UriFormatException(string.Format(Resources.UriFormatException, $"{uri.Scheme}://{{configuration-name}}/{{topic}}", uri));
            }

            ConfigurationName = Uri.Host;
            Queue = Uri.Segments[1];
        }

        public QueueUri(string uri) : this(new Uri(uri))
        {
        }

        public string Queue { get; }

        public string ConfigurationName { get; }

        public void SchemeInvariant(string scheme)
        {
            Guard.AgainstNullOrEmptyString(scheme, nameof(scheme));

            if (Uri.Scheme.Equals(scheme, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            throw new InvalidSchemeException(Uri.Scheme, Uri.ToString());
        }
    }
}