using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class DefaultConfigurator
	{
		private readonly ServiceBusConfiguration _configuration = new ServiceBusConfiguration();

		public DefaultConfigurator MessageSerializer(ISerializer serializer)
		{
			Guard.AgainstNull(serializer, "serializer");

			_configuration.Serializer = serializer;

			return this;
		}

		public DefaultConfigurator MessageHandlerFactory(IMessageHandlerFactory messageHandlerFactory)
		{
			Guard.AgainstNull(messageHandlerFactory, "messageHandlerFactory");

			_configuration.MessageHandlerFactory = messageHandlerFactory;

			return this;
		}

		public DefaultConfigurator AddCompressionAlgorithm(ICompressionAlgorithm algorithm)
		{
			Guard.AgainstNull(algorithm, "algorithm");

			_configuration.AddCompressionAlgorithm(algorithm);

			return this;
		}

		public DefaultConfigurator AddEnryptionAlgorithm(IEncryptionAlgorithm algorithm)
		{
			Guard.AgainstNull(algorithm, "algorithm");

			_configuration.AddEncryptionAlgorithm(algorithm);

			return this;
		}

		public DefaultConfigurator SubscriptionManager(ISubscriptionManager manager)
		{
			Guard.AgainstNull(manager, "manager");

			_configuration.SubscriptionManager = manager;

			return this;
		}

		public DefaultConfigurator AddModule(IModule module)
		{
			Guard.AgainstNull(module, "module");

			_configuration.Modules.Add(module);

			return this;
		}

		public DefaultConfigurator Policy(IServiceBusPolicy policy)
		{
			Guard.AgainstNull(policy, "policy");

			_configuration.Policy = policy;

			return this;
		}

		public DefaultConfigurator ThreadActivityFactory(IThreadActivityFactory factory)
		{
			Guard.AgainstNull(factory, "factory");

			_configuration.ThreadActivityFactory = factory;

			return this;
		}

		public DefaultConfigurator PipelineFactory(IPipelineFactory pipelineFactory)
		{
			Guard.AgainstNull(pipelineFactory, "pipelineFactory");

			_configuration.PipelineFactory = pipelineFactory;

			return this;
		}

		public DefaultConfigurator TransactionScopeFactory(ITransactionScopeFactory transactionScopeFactory)
		{
			Guard.AgainstNull(transactionScopeFactory, "transactionScopeFactory");

			_configuration.TransactionScopeFactory = transactionScopeFactory;

			return this;
		}

		public DefaultConfigurator MessageRouteProvider(IMessageRouteProvider messageRouteProvider)
		{
			Guard.AgainstNull(messageRouteProvider, "messageRouteProvider");

			_configuration.MessageRouteProvider = messageRouteProvider;

			return this;
		}

		public DefaultConfigurator IdempotenceService(IIdempotenceService idempotenceService)
		{
			Guard.AgainstNull(idempotenceService, "idempotenceService");

			_configuration.IdempotenceService = idempotenceService;

			return this;
		}

		public DefaultConfigurator UriResolver(IUriResolver uriResolver)
		{
			Guard.AgainstNull(uriResolver, "uriResolver");

			_configuration.UriResolver = uriResolver;

			return this;
		}

		public IServiceBusConfiguration Configuration()
		{
			new QueueManagerConfigurator().Apply(_configuration);

			new CoreConfigurator().Apply(_configuration);
			new ControlInboxConfigurator().Apply(_configuration);
			new InboxConfigurator().Apply(_configuration);
			new OutboxConfigurator().Apply(_configuration);
			new WorkerConfigurator().Apply(_configuration);
			new ModuleConfigurator().Apply(_configuration);
			new MessageRouteProviderConfigurator().Apply(_configuration);
			new UriResolverConfigurator().Apply(_configuration);

			return _configuration;
		}
	}
}