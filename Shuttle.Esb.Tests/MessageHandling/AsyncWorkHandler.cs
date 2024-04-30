using System;
using System.Threading.Tasks;

namespace Shuttle.Esb.Tests.MessageHandling;

public class AsyncWorkHandler : IAsyncMessageHandler<WorkMessage>
{
    public async Task ProcessMessageAsync(IHandlerContext<WorkMessage> context)
    {
        Console.WriteLine($@"[work-message] : guid = {context.Message.Guid}");

        await Task.CompletedTask;
    }
}