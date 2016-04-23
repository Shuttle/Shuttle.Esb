using System;

namespace Shuttle.Esb
{
	public interface IQueueFactory
	{
		string Scheme { get; }
		IQueue Create(Uri uri);
		bool CanCreate(Uri uri);
	}
}