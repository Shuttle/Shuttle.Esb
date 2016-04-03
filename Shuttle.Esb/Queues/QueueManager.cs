using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class QueueManager : IQueueManager, IRequireInitialization, IDisposable
	{
		private readonly List<IQueueFactory> _queueFactories = new List<IQueueFactory>();
		private readonly List<IQueue> _queues = new List<IQueue>();

		private readonly ILog _log;
		private static readonly object Padlock = new object();
		private IUriResolver _uriResolver;

		public QueueManager()
		{
			_log = Log.For(this);
		}

		public void Dispose()
		{
			foreach (var queueFactory in _queueFactories)
			{
				queueFactory.AttemptDispose();
			}

			foreach (var queue in _queues)
			{
				queue.AttemptDispose();
			}

			_queueFactories.Clear();
			_queues.Clear();
		}

		public IQueueFactory GetQueueFactory(string scheme)
		{
			Uri uri;

			return Uri.TryCreate(scheme, UriKind.Absolute, out uri)
				? GetQueueFactory(uri)
				: _queueFactories.Find(factory => factory.Scheme.Equals(scheme, StringComparison.InvariantCultureIgnoreCase));
		}

		public IQueueFactory GetQueueFactory(Uri uri)
		{
			foreach (var factory in _queueFactories.Where(factory => factory.CanCreate(uri)))
			{
				return factory;
			}

			throw new QueueFactoryNotFoundException(uri.Scheme);
		}

		public IQueue GetQueue(string uri)
		{
			Guard.AgainstNullOrEmptyString(uri, "uri");

			var queue =
				_queues.Find(
					candidate => Find(candidate, uri));

			if (queue != null)
			{
				return queue;
			}

			lock (Padlock)
			{
				queue =
					_queues.Find(
						candidate => Find(candidate, uri));

				if (queue != null)
				{
					return queue;
				}

				var queueUri = new Uri(uri);

				if (queueUri.Scheme.Equals("resolver"))
				{
					if (_uriResolver == null)
					{
						throw new InvalidOperationException(string.Format(EsbResources.NoUriResolverException, uri));
					}

					var resolvedQueueUri = _uriResolver.Get(uri);

					if (resolvedQueueUri == null)
					{
						throw new KeyNotFoundException(string.Format(EsbResources.UriNameNotFoundException, _uriResolver.GetType().FullName,
							uri));
					}

					queue = new ResolvedQueue(CreateQueue(GetQueueFactory(resolvedQueueUri), resolvedQueueUri), queueUri);
				}
				else
				{
					queue = CreateQueue(GetQueueFactory(queueUri), queueUri);
				}

				_queues.Add(queue);

				return queue;
			}
		}

		public IQueue CreateQueue(string uri)
		{
			return CreateQueue(new Uri(uri));
		}

		public IQueue CreateQueue(Uri uri)
		{
			return GetQueueFactory(uri).Create(uri);
		}

		public IEnumerable<IQueueFactory> QueueFactories()
		{
			return new ReadOnlyCollection<IQueueFactory>(_queueFactories);
		}

		public void RegisterQueueFactory(IQueueFactory queueFactory)
		{
			Guard.AgainstNull(queueFactory, "queueFactory");

			var factory = GetQueueFactory(queueFactory.Scheme);

			if (factory != null)
			{
				_queueFactories.Remove(factory);

				_log.Warning(string.Format(EsbResources.DuplicateQueueFactoryReplaced, queueFactory.Scheme,
					factory.GetType().FullName, queueFactory.GetType().FullName));
			}

			_queueFactories.Add(queueFactory);
		}

		public bool ContainsQueueFactory(string scheme)
		{
			return GetQueueFactory(scheme) != null;
		}

		public void ScanForQueueFactories()
		{
			foreach (var type in new ReflectionService().GetTypes<IQueueFactory>())
			{
				RegisterQueueFactory(type);
			}
		}

		public void RegisterQueueFactory(Type type)
		{
			try
			{
				type.AssertDefaultConstructor(string.Format(EsbResources.DefaultConstructorRequired, "IQueueFactory", type.FullName));

				var instance = (IQueueFactory)Activator.CreateInstance(type);

				if (!ContainsQueueFactory(instance.Scheme))
				{
					RegisterQueueFactory(instance);
				}
			}
			catch (Exception ex)
			{
				throw new EsbConfigurationException(string.Format(EsbResources.QueueFactoryInstantiationException, type.FullName, ex.AllMessages()));
			}
		}

		private IQueue CreateQueue(IQueueFactory queueFactory, Uri queueUri)
		{
			var result = queueFactory.Create(queueUri);

			Guard.AgainstNull(result,
				string.Format(EsbResources.QueueFactoryCreatedNullQueue, queueFactory.GetType().FullName, queueUri));

			return result;
		}

		private bool Find(IQueue candidate, string uri)
		{
			try
			{
				return candidate.Uri.ToString().Equals(uri, StringComparison.InvariantCultureIgnoreCase);
			}
			catch (Exception ex)
			{
				var candidateTypeName = "(candidate is null)";
				var candidateUri = "(candidate is null)";

				if (candidate != null)
				{
					candidateTypeName = candidate.GetType().FullName;
					candidateUri = candidate.Uri != null
						? candidate.Uri.ToString()
						: "(candidate.Uri is null)";
				}

				_log.Error(string.Format(EsbResources.FindQueueException, candidateTypeName, candidateUri,
					uri ?? "(comparison uri is null)", ex.AllMessages()));

				return false;
			}
		}

		public void CreatePhysicalQueues(IServiceBusConfiguration configuration)
		{
			if (configuration.HasInbox)
			{
				CreateQueues(configuration.Inbox);

				if (configuration.Inbox.HasDeferredQueue)
				{
					configuration.Inbox.DeferredQueue.AttemptCreate();
				}
			}

			if (configuration.HasOutbox)
			{
				CreateQueues(configuration.Outbox);
			}

			if (configuration.HasControlInbox)
			{
				CreateQueues(configuration.ControlInbox);
			}
		}
		
		private void CreateQueues(IWorkQueueConfiguration workQueueConfiguration)
		{
			workQueueConfiguration.WorkQueue.AttemptCreate();

			var errorQueueConfiguration = workQueueConfiguration as IErrorQueueConfiguration;

			if (errorQueueConfiguration != null)
			{
				errorQueueConfiguration.ErrorQueue.AttemptCreate();
			}
		}

		public void Initialize(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "bus");

			_uriResolver = bus.Configuration.UriResolver;
		}
	}
}