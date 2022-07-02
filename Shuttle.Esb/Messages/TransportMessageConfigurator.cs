﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class TransportMessageConfigurator
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

        public TransportMessageConfigurator(object message)
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

        public TransportMessage TransportMessage(IServiceBusConfiguration configuration,
            IIdentityProvider identityProvider)
        {
            Guard.AgainstNull(identityProvider, nameof(identityProvider));

            if (_local && !configuration.HasInbox)
            {
                throw new InvalidOperationException(Resources.SendToSelfException);
            }

            var identity = identityProvider.Get();

            var result = new TransportMessage
            {
                RecipientInboxWorkQueueUri = _local
                    ? configuration.Inbox.WorkQueue.Uri.ToString()
                    : _recipientInboxWorkQueueUri,
                SenderInboxWorkQueueUri = string.IsNullOrEmpty(_sendInboxWorkQueueUri)
                    ? configuration.HasInbox
                        ? configuration.Inbox.WorkQueue.Uri.ToString()
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
                EncryptionAlgorithm = _encryptionAlgorithm ?? configuration.EncryptionAlgorithm,
                CompressionAlgorithm = _compressionAlgorithm ?? configuration.CompressionAlgorithm,
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

        public TransportMessageConfigurator WithPriority(int priority)
        {
            _priority = priority;

            return this;
        }

        public TransportMessageConfigurator WithEncryption(string encryption)
        {
            _encryptionAlgorithm = encryption;

            return this;
        }

        public TransportMessageConfigurator WithCompression(string compression)
        {
            _compressionAlgorithm = compression;

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
                throw new InvalidOperationException(Resources.SendReplyException);
            }

            _local = false;

            WithRecipient(_transportMessageReceived.SenderInboxWorkQueueUri);

            return this;
        }
    }
}