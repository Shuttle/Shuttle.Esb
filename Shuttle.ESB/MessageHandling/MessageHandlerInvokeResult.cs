namespace Shuttle.Esb
{
    public class MessageHandlerInvokeResult
    {
        private MessageHandlerInvokeResult(bool invoked, object messageHandler)
        {
            Invoked = invoked;
            MessageHandler = messageHandler;
        }

        public bool Invoked { get; private set; }
        public object MessageHandler { get; private set; }

        public static MessageHandlerInvokeResult InvokedHandler(object handler)
        {
            return new MessageHandlerInvokeResult(true, handler);
        }

        public static MessageHandlerInvokeResult InvokeFailure()
        {
            return new MessageHandlerInvokeResult(false, null);
        }
    }
}