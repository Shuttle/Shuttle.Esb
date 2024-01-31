using System;

namespace Shuttle.Esb.Tests.MessageHandling;

public class WorkHandler : IMessageHandler<WorkMessage>
{
    public void ProcessMessage(IHandlerContext<WorkMessage> context)
    {
        Console.WriteLine($@"[work-message] : guid = {context.Message.Guid}");
    }
}