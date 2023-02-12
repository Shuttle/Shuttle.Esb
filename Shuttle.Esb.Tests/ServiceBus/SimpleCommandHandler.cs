using System;
using System.Threading.Tasks;

namespace Shuttle.Esb.Tests
{
    public class SimpleCommandHandler : IMessageHandler<SimpleCommand>
    {
        public async Task ProcessMessage(IHandlerContext<SimpleCommand> context)
        {
            Console.WriteLine($@"Handled SimpleCommand with name '{context.Message.Name}.");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}