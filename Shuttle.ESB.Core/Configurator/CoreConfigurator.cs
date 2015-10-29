using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class CoreConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

		    var section = ServiceBusConfiguration.ServiceBusSection;

		    if (section == null)
			{
				configuration.RemoveMessagesNotHandled = false;

				return;
			}

		    configuration.CreateQueues = section.CreateQueues;
			configuration.RemoveMessagesNotHandled = section.RemoveMessagesNotHandled;
			configuration.CompressionAlgorithm = section.CompressionAlgorithm;
			configuration.EncryptionAlgorithm = section.EncryptionAlgorithm;

			var transactionScopeElement = section.TransactionScope;

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