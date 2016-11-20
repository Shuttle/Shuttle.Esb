using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public interface IServiceBus :
		IMessageSender,
		IDisposable
	{
		IServiceBus Start();
		void Stop();

		bool Started { get; }

		IServiceBusConfiguration Configuration { get; }
		IServiceBusEvents Events { get; }
	}
}