using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Tests
{
	public class ServiceBusRoutingModule :
		IModule,
		IPipelineObserver<OnAfterDeserializeMessage>
	{
		public SimpleCommand SimpleCommand { get; private set; }

		public void Initialize(IServiceBus bus)
		{
			bus.Events.PipelineCreated += PipelineCreated;
		}

		private void PipelineCreated(object sender, PipelineEventArgs e)
		{
			if (!e.Pipeline.GetType()
				  .FullName.Equals(typeof(InboxMessagePipeline).FullName, StringComparison.InvariantCultureIgnoreCase))
			{
				return;
			}

			e.Pipeline.RegisterObserver(this);
		}

		public void Execute(OnAfterDeserializeMessage pipelineEvent)
		{
			SimpleCommand = (SimpleCommand)pipelineEvent.Pipeline.State.GetMessage();
		}
	}
}