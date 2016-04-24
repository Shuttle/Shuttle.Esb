using System;

namespace Shuttle.Esb.Tests
{
	public class SimpleCommandHandler : IMessageHandler<SimpleCommand>
	{
		public void ProcessMessage(IHandlerContext<SimpleCommand> context)
		{
			Console.WriteLine("Handled SimpleCommand with name '{0}.", context.Message.Name);
		}

		public bool IsReusable {
			get { return true; }
		}
	}
}