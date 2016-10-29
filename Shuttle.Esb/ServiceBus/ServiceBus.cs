using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class ServiceBus : IServiceBus
	{
		private readonly IMessageSender _messageSender;

		private IProcessorThreadPool _controlThreadPool;
		private IProcessorThreadPool _inboxThreadPool;
		private IProcessorThreadPool _outboxThreadPool;
		private IProcessorThreadPool _deferredMessageThreadPool;

		public ServiceBus(IServiceBusConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			Configuration = configuration;

			Events = new ServiceBusEvents();

			_messageSender = new MessageSender(this);
		}

		public IServiceBusConfiguration Configuration { get; private set; }
		public IServiceBusEvents Events { get; private set; }

		public IServiceBus Start()
		{
			if (Started)
			{
				throw new ApplicationException(EsbResources.ServiceBusInstanceAlreadyStarted);
			}

			GuardAgainstInvalidConfiguration();

			// cannot be in startup pipeline as some modules may need to observe the startup pipeline
			foreach (var module in Configuration.Modules)
			{
                module.AttemptDependencyInjection<IServiceBus>(this);

				module.Start(Configuration.PipelineFactory);
			}

		    Configuration.PipelineFactory.PipelineCreated += (sender, args) =>
		    {
		        args.Pipeline.State.Add<IServiceBus>(this);
                args.Pipeline.AttemptDependencyInjection<IServiceBus>(this);
		    };

            var startupPipeline = Configuration.PipelineFactory.GetPipeline<StartupPipeline>();

            startupPipeline.Execute();

			_inboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("InboxThreadPool");
			_controlThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("ControlInboxThreadPool");
			_outboxThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("OutboxThreadPool");
			_deferredMessageThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("DeferredMessageThreadPool");

			Started = true;

			return this;
		}

		private void GuardAgainstInvalidConfiguration()
		{
			Guard.Against<EsbConfigurationException>(Configuration.Serializer == null, EsbResources.NoSerializerException);

			Guard.Against<EsbConfigurationException>(Configuration.MessageHandlerFactory == null,
				EsbResources.NoMessageHandlerFactoryException);

			Guard.Against<WorkerException>(Configuration.IsWorker && !Configuration.HasInbox,
				EsbResources.WorkerRequiresInboxException);

			if (Configuration.HasInbox)
			{
				Guard.Against<EsbConfigurationException>(Configuration.Inbox.WorkQueue == null,
					string.Format(EsbResources.RequiredQueueMissing, "Inbox.WorkQueue"));

				Guard.Against<EsbConfigurationException>(Configuration.Inbox.ErrorQueue == null,
					string.Format(EsbResources.RequiredQueueMissing, "Inbox.ErrorQueue"));
			}

			if (Configuration.HasOutbox)
			{
				Guard.Against<EsbConfigurationException>(Configuration.Outbox.WorkQueue == null,
					string.Format(EsbResources.RequiredQueueMissing, "Outbox.WorkQueue"));

				Guard.Against<EsbConfigurationException>(Configuration.Outbox.ErrorQueue == null,
					string.Format(EsbResources.RequiredQueueMissing, "Outbox.ErrorQueue"));
			}

			if (Configuration.HasControlInbox)
			{
				Guard.Against<EsbConfigurationException>(Configuration.ControlInbox.WorkQueue == null,
					string.Format(EsbResources.RequiredQueueMissing, "ControlInbox.WorkQueue"));

				Guard.Against<EsbConfigurationException>(Configuration.ControlInbox.ErrorQueue == null,
					string.Format(EsbResources.RequiredQueueMissing, "ControlInbox.ErrorQueue"));
			}
		}

		public void Stop()
		{
			if (!Started)
			{
				return;
			}

			Configuration.Modules.AttemptDispose();

			if (Configuration.HasInbox)
			{
				if (Configuration.Inbox.HasDeferredQueue)
				{
					_deferredMessageThreadPool.Dispose();
				}

				_inboxThreadPool.Dispose();
			}

			if (Configuration.HasControlInbox)
			{
				_controlThreadPool.Dispose();
			}

			if (Configuration.HasOutbox)
			{
				_outboxThreadPool.Dispose();
			}

			Configuration.QueueManager.AttemptDispose();

			Started = false;
		}

		public bool Started { get; private set; }

		public void Dispose()
		{
			Stop();
		}

		public static IServiceBus Create()
		{
			return Create(null);
		}

		public static IServiceBus Create(Action<DefaultConfigurator> configure)
		{
			var configurator = new DefaultConfigurator();

			if (configure != null)
			{
				configure.Invoke(configurator);
			}

			return new ServiceBus(configurator.Configuration());
		}

		public TransportMessage CreateTransportMessage(object message, Action<TransportMessageConfigurator> configure)
		{
			return _messageSender.CreateTransportMessage(message, configure);
		}

		public void Dispatch(TransportMessage transportMessage)
		{
			_messageSender.Dispatch(transportMessage);
		}

		public TransportMessage Send(object message)
		{
			return _messageSender.Send(message);
		}

		public TransportMessage Send(object message, Action<TransportMessageConfigurator> configure)
		{
			return _messageSender.Send(message, configure);
		}

		public IEnumerable<TransportMessage> Publish(object message)
		{
			return _messageSender.Publish(message);
		}

		public IEnumerable<TransportMessage> Publish(object message, Action<TransportMessageConfigurator> configure)
		{
			return _messageSender.Publish(message, configure);
		}
	}
}