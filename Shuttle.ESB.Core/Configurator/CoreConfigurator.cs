using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class CoreConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			if (ServiceBusConfiguration.ServiceBusSection == null)
			{
				configuration.RemoveMessagesNotHandled = false;

				return;
			}

			configuration.RemoveMessagesNotHandled = ServiceBusConfiguration.ServiceBusSection.RemoveMessagesNotHandled;
			configuration.CompressionAlgorithm = ServiceBusConfiguration.ServiceBusSection.CompressionAlgorithm;
			configuration.EncryptionAlgorithm = ServiceBusConfiguration.ServiceBusSection.EncryptionAlgorithm;

			var transactionScopeElement = ServiceBusConfiguration.ServiceBusSection.TransactionScope;

			configuration.TransactionScope = transactionScopeElement != null
				? new TransactionScopeConfiguration
				{
					Enabled = transactionScopeElement.Enabled,
					IsolationLevel = transactionScopeElement.IsolationLevel,
					TimeoutSeconds = transactionScopeElement.TimeoutSeconds
				}
				: new TransactionScopeConfiguration();
		}
	}
}