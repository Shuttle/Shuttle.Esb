using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
    public static class IdempotenceServiceExtensions
    {
        public static void AccessException(this IIdempotenceService service, ILog log, Exception ex, IPipeline pipeline)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNull(log, nameof(log));
            Guard.AgainstNull(ex, nameof(ex));
            Guard.AgainstNull(pipeline, nameof(pipeline));

            log.Fatal(string.Format(EsbResources.FatalIdempotenceServiceException, service.GetType().FullName,
                ex.AllMessages()));

            pipeline.Abort();
        }
    }
}