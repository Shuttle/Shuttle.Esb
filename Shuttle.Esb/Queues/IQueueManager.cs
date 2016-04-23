using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
	public interface IQueueManager
	{
		IQueueFactory GetQueueFactory(string scheme);
		IQueueFactory GetQueueFactory(Uri uri);
		IQueue GetQueue(string uri);
		IQueue CreateQueue(string uri);
		IQueue CreateQueue(Uri uri);
		IEnumerable<IQueueFactory> QueueFactories();
		void RegisterQueueFactory(IQueueFactory queueFactory);
		bool ContainsQueueFactory(string scheme);
		void ScanForQueueFactories();
		void RegisterQueueFactory(Type type);
		void CreatePhysicalQueues(IServiceBusConfiguration configuration);
	}
}