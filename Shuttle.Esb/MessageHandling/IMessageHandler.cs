namespace Shuttle.Esb
{
	public interface IMessageHandler
	{
		bool IsReusable { get; }
	}

	public interface IMessageHandler<in T> : IMessageHandler where T : class
	{
		void ProcessMessage(IHandlerContext<T> context);
	}
}