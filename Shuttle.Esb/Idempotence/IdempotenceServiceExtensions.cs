using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public static class IdempotenceServiceExtensions
	{
		public static void AccessException(this IIdempotenceService service, ILog log, Exception ex, Pipeline pipeline)
		{
			Guard.AgainstNull(service, "service");
			Guard.AgainstNull(log, "log");
			Guard.AgainstNull(ex, "ex");
			Guard.AgainstNull(pipeline, "pipeline");

			log.Fatal(string.Format(EsbResources.FatalIdempotenceServiceException, service.GetType().FullName,
				ex.AllMessages()));

			pipeline.Abort();
		}
	}
}