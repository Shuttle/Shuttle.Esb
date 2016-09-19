using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class CoreConfigurator : IConfigurator
	{
		public void Apply(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			var section = ServiceBusConfiguration.ServiceBusSection;

			if (section == null)
			{
				return;
			}

			configuration.CreateQueues = section.CreateQueues;
			configuration.CacheIdentity = section.CacheIdentity;
			configuration.RegisterHandlers = section.RegisterHandlers;
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