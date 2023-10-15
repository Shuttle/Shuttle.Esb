using System;
using System.Threading.Tasks;
using Shuttle.Esb.Tests.Messages;

namespace Shuttle.Esb.Tests
{
    public class SimpleCommandHandler : IAsyncMessageHandler<SimpleCommand>
    {
        public async Task ProcessMessage(IHandlerContext<SimpleCommand> context)
        {
            Console.WriteLine($@"Handled SimpleCommand with name '{context.Message.Name}.");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}