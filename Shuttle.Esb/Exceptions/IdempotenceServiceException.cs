using System;

namespace Shuttle.Esb
{
	public class IdempotenceServiceException : Exception
	{
		public IdempotenceServiceException(string message) : base(message)
		{
		}
	}
}