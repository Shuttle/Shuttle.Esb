using System;
using System.Collections.Generic;
using System.Security.Principal;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public class TransportMessageConfigurator
	{
		private bool _local;
		private string _sendInboxWorkQueueUri;
		private string _recipientInboxWorkQueueUri;
		private TransportMessage _transportMessageReceived;
		private DateTime _ignoreTillDate;
		private DateTime _expiryDate;
		private string _correlationId;
	    private readonly string _anonymousName = WindowsIdentity.GetAnonymous().Name;

        public List<TransportHeader> Headers { get; set; }
		public object Message { get; private set; }

		public TransportMessageConfigurator(object message)
		{
			Guard.AgainstNull(message, "message");

			Headers = new List<TransportHeader>();
			Message = message;

			_correlationId = string.Empty;
			_ignoreTillDate = DateTime.MinValue;
			_expiryDate = DateTime.MaxValue;
			_recipientInboxWorkQueueUri = null;
			_local = false;
		}

		public TransportMessage TransportMessage(IServiceBusConfiguration configuration, IIdentityProvider identityProvider)
		{
            Guard.AgainstNull(identityProvider, "identityProvider");

			if (_local && !configuration.HasInbox)
			{
				throw new InvalidOperationException(EsbResources.SendToSelfException);
			}

		    var identity = identityProvider.Get(); 

			var result = new TransportMessage
			{
				RecipientInboxWorkQueueUri = _local ? configuration.Inbox.WorkQueue.Uri.ToString() : _recipientInboxWorkQueueUri,
				SenderInboxWorkQueueUri = string.IsNullOrEmpty(_sendInboxWorkQueueUri)
					? configuration.HasInbox
						? configuration.Inbox.WorkQueue.Uri.ToString()
						: string.Empty
					: _sendInboxWorkQueueUri,
				PrincipalIdentityName = identity != null
					? identity.Name
					: _anonymousName,
				IgnoreTillDate = _ignoreTillDate,
				ExpiryDate = _expiryDate,
				MessageType = Message.GetType().FullName,
				AssemblyQualifiedName = Message.GetType().AssemblyQualifiedName,
				EncryptionAlgorithm = configuration.EncryptionAlgorithm,
				CompressionAlgorithm = configuration.CompressionAlgorithm,
                MessageReceivedId = HasTransportMessageReceived ? _transportMessageReceived.MessageId : Guid.Empty,
                CorrelationId = _correlationId,
				SendDate = DateTime.Now
			};

			result.Headers.Merge(Headers);

			return result;
		}

        public bool HasTransportMessageReceived
		{
			get { return _transportMessageReceived != null; }
		}

        public void TransportMessageReceived(TransportMessage transportMessageReceived)
        {
            Guard.AgainstNull(transportMessageReceived, "transportMessageReceived");

            _transportMessageReceived = transportMessageReceived;

            Headers.Merge(transportMessageReceived.Headers);
            _correlationId = transportMessageReceived.CorrelationId;
        }

        public TransportMessageConfigurator Defer(DateTime ignoreTillDate)
		{
			_ignoreTillDate = ignoreTillDate;

			return this;
		}

		public TransportMessageConfigurator WillExpire(DateTime expiryDate)
		{
			_expiryDate = expiryDate;

			return this;
		}

		public TransportMessageConfigurator WithCorrelationId(string correlationId)
		{
			_correlationId = correlationId;

			return this;
		}

		public TransportMessageConfigurator WithRecipient(IQueue queue)
		{
			return WithRecipient(queue.Uri.ToString());
		}

		public TransportMessageConfigurator WithRecipient(Uri uri)
		{
			return WithRecipient(uri.ToString());
		}

		public TransportMessageConfigurator WithRecipient(string uri)
		{
			_local = false;

			_recipientInboxWorkQueueUri = uri;

			return this;
		}

		public TransportMessageConfigurator WithSender(IQueue queue)
		{
			return WithSender(queue.Uri.ToString());
		}

		public TransportMessageConfigurator WithSender(Uri uri)
		{
			return WithSender(uri.ToString());
		}

		public TransportMessageConfigurator WithSender(string uri)
		{
			_sendInboxWorkQueueUri = uri;

			return this;
		}

		public TransportMessageConfigurator Local()
		{
			_local = true;

			return this;
		}

		public TransportMessageConfigurator Reply()
		{
			if (!HasTransportMessageReceived || string.IsNullOrEmpty(_transportMessageReceived.SenderInboxWorkQueueUri))
			{
				throw new InvalidOperationException(EsbResources.SendReplyException);
			}

			_local = false;

			WithRecipient(_transportMessageReceived.SenderInboxWorkQueueUri);

			return this;
		}
	}
}