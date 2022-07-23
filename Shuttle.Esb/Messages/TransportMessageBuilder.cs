using System;
using System.Collections.Generic;
using System.Security.Principal;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class TransportMessageBuilder
    {
        private static readonly string AnonymousName = new GenericIdentity(Environment.UserDomainName + "\\" + Environment.UserName, "Anonymous").Name;
        private string _correlationId;
        private DateTime _expiryDate;
        private int _priority;
        private string _encryptionAlgorithm;
        private string _compressionAlgorithm;
        private DateTime _ignoreTillDate;
        private bool _local;
        private string _recipientInboxWorkQueueUri;
        private string _sendInboxWorkQueueUri;
        private TransportMessage _transportMessageReceived;

        public TransportMessageBuilder(object message)
        {
            Guard.AgainstNull(message, nameof(message));

            Headers = new List<TransportHeader>();
            Message = message;

            _correlationId = string.Empty;
            _ignoreTillDate = DateTime.MinValue;
            _expiryDate = DateTime.MaxValue;
            _recipientInboxWorkQueueUri = null;
            _local = false;
        }

        public List<TransportHeader> Headers { get; set; }
        public object Message { get; }

        public bool HasTransportMessageReceived => _transportMessageReceived != null;

        public TransportMessage TransportMessage(ServiceBusOptions serviceBusOptions, IServiceBusConfiguration serviceBusConfiguration, IIdentityProvider identityProvider)
        {
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(serviceBusOptions, nameof(serviceBusOptions));
            Guard.AgainstNull(identityProvider, nameof(identityProvider));

            if (_local && !serviceBusConfiguration.HasInbox())
            {
                throw new InvalidOperationException(Resources.SendToSelfException);
            }

            if (_ignoreTillDate > DateTime.Now &&
                serviceBusConfiguration.HasInbox() &&
                serviceBusConfiguration.Inbox.WorkQueue.IsStream)
            {
                throw new InvalidOperationException(Resources.DeferStreamException);
            }

            var identity = identityProvider.Get();

            var result = new TransportMessage
            {
                RecipientInboxWorkQueueUri = _local
                    ? serviceBusConfiguration.Inbox.WorkQueue.Uri.ToString()
                    : _recipientInboxWorkQueueUri,
                SenderInboxWorkQueueUri = string.IsNullOrEmpty(_sendInboxWorkQueueUri)
                    ? serviceBusConfiguration.HasInbox()
                        ? serviceBusConfiguration.Inbox.WorkQueue.Uri.ToString()
                        : string.Empty
                    : _sendInboxWorkQueueUri,
                PrincipalIdentityName = identity != null
                    ? identity.Name
                    : AnonymousName,
                IgnoreTillDate = _ignoreTillDate,
                ExpiryDate = _expiryDate,
                Priority = _priority,
                MessageType = Message.GetType().FullName,
                AssemblyQualifiedName = Message.GetType().AssemblyQualifiedName,
                EncryptionAlgorithm = _encryptionAlgorithm ?? serviceBusOptions.EncryptionAlgorithm,
                CompressionAlgorithm = _compressionAlgorithm ?? serviceBusOptions.CompressionAlgorithm,
                MessageReceivedId = HasTransportMessageReceived ? _transportMessageReceived.MessageId : Guid.Empty,
                CorrelationId = _correlationId,
                SendDate = DateTime.Now
            };

            result.Headers.Merge(Headers);

            return result;
        }

        public void TransportMessageReceived(TransportMessage transportMessageReceived)
        {
            Guard.AgainstNull(transportMessageReceived, nameof(transportMessageReceived));

            _transportMessageReceived = transportMessageReceived;

            Headers.Merge(transportMessageReceived.Headers);
            _correlationId = transportMessageReceived.CorrelationId;
        }

        public TransportMessageBuilder Defer(DateTime ignoreTillDate)
        {
            _ignoreTillDate = ignoreTillDate;

            return this;
        }

        public TransportMessageBuilder WillExpire(DateTime expiryDate)
        {
            _expiryDate = expiryDate;

            return this;
        }

        public TransportMessageBuilder WithPriority(int priority)
        {
            _priority = priority;

            return this;
        }

        public TransportMessageBuilder WithEncryption(string encryption)
        {
            _encryptionAlgorithm = encryption;

            return this;
        }

        public TransportMessageBuilder WithCompression(string compression)
        {
            _compressionAlgorithm = compression;

            return this;
        }

        public TransportMessageBuilder WithCorrelationId(string correlationId)
        {
            _correlationId = correlationId;

            return this;
        }

        public TransportMessageBuilder WithRecipient(IQueue queue)
        {
            return WithRecipient(queue.Uri.ToString());
        }

        public TransportMessageBuilder WithRecipient(Uri uri)
        {
            return WithRecipient(uri.ToString());
        }

        public TransportMessageBuilder WithRecipient(string uri)
        {
            _local = false;

            _recipientInboxWorkQueueUri = uri;

            return this;
        }

        public TransportMessageBuilder WithSender(IQueue queue)
        {
            return WithSender(queue.Uri.ToString());
        }

        public TransportMessageBuilder WithSender(Uri uri)
        {
            return WithSender(uri.ToString());
        }

        public TransportMessageBuilder WithSender(string uri)
        {
            _sendInboxWorkQueueUri = uri;

            return this;
        }

        public TransportMessageBuilder Local()
        {
            _local = true;

            return this;
        }

        public TransportMessageBuilder Reply()
        {
            if (!HasTransportMessageReceived || string.IsNullOrEmpty(_transportMessageReceived.SenderInboxWorkQueueUri))
            {
                throw new InvalidOperationException(Resources.SendReplyException);
            }

            _local = false;

            WithRecipient(_transportMessageReceived.SenderInboxWorkQueueUri);

            return this;
        }
    }
}