using System;
using NUnit.Framework;

namespace Shuttle.ESB.Core.Tests
{
	public class NullQueueFactory : IQueueFactory
	{
		public NullQueueFactory()
		{
			Scheme = "null-queue";
		}

		public string Scheme { get; private set; }
		
		public IQueue Create(Uri uri)
		{
			Guard.ArgumentNotNull(uri, "uri");

			return new NullQueue(uri);
		}

		public bool CanCreate(Uri uri)
		{
			return uri.Scheme.Equals(Scheme, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}