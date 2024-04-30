using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class IQueueServiceExtensions
    {
        public static IQueue Get(this IQueueService queueService, string uri)
        {
            try
            {
                return Guard.AgainstNull(queueService, nameof(queueService)).Get(new Uri(Guard.AgainstNullOrEmptyString(uri, nameof(uri))));
            }
            catch (UriFormatException ex)
            {
                throw new UriFormatException($"{ex.Message} / uri = '{uri}'");
            }
        }

        public static bool Contains(this IQueueService queueService, string uri)
        {
            try
            {
                return Guard.AgainstNull(queueService, nameof(queueService)).Contains(new Uri(Guard.AgainstNullOrEmptyString(uri, nameof(uri))));
            }
            catch (UriFormatException ex)
            {
                throw new UriFormatException($"{ex.Message} / uri = '{uri}'");
            }
        }
    }
}