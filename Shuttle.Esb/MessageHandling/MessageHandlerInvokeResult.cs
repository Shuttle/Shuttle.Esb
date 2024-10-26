namespace Shuttle.Esb;

public class MessageHandlerInvokeResult
{
    private MessageHandlerInvokeResult(string messageHandlerAssemblyQualifiedName)
    {
        MessageHandlerAssemblyQualifiedName = messageHandlerAssemblyQualifiedName;
    }

    public bool Invoked => !string.IsNullOrEmpty(MessageHandlerAssemblyQualifiedName);
    public string MessageHandlerAssemblyQualifiedName { get; }

    public static MessageHandlerInvokeResult InvokedHandler(string assemblyQualifiedName)
    {
        return new(assemblyQualifiedName);
    }

    public static MessageHandlerInvokeResult MissingHandler()
    {
        return new(string.Empty);
    }
}