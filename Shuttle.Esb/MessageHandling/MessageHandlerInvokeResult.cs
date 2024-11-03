namespace Shuttle.Esb;

public class MessageHandlerInvokeResult
{
    public bool Invoked { get; init; }

    public static MessageHandlerInvokeResult InvokedHandler()
    {
        return new() { Invoked = true };
    }

    public static MessageHandlerInvokeResult MissingHandler()
    {
        return new();
    }
}