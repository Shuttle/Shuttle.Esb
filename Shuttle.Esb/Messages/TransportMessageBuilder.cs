using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class TransportMessageBuilder
    {
        private readonly TransportMessage _transportMessage;
        private TransportMessage _transportMessageReceived;

        public TransportMessageBuilder(TransportMessage transportMessage)
        {
            Guard.AgainstNull(transportMessage, nameof(transportMessage));

            _transportMessage = transportMessage;

            Headers = new List<TransportHeader>();
        }

        public List<TransportHeader> Headers { get; set; }
        public bool HasTransportMessageReceived => _transportMessageReceived != null;

        public bool ShouldSendLocal { get; private set; }
        public bool HasRecipient => !string.IsNullOrWhiteSpace(_transportMessage.RecipientInboxWorkQueueUri);

        public void Received(TransportMessage transportMessageReceived)
        {
            Guard.AgainstNull(transportMessageReceived, nameof(transportMessageReceived));

            _transportMessageReceived = transportMessageReceived;
            _transportMessage.CorrelationId = transportMessageReceived.CorrelationId;

            Headers.Merge(transportMessageReceived.Headers);
        }

        public TransportMessageBuilder Defer(DateTime ignoreTillDate)
        {
            _transportMessage.IgnoreTillDate = ignoreTillDate;

            return this;
        }

        public TransportMessageBuilder WillExpire(DateTime expiryDate)
        {
            _transportMessage.ExpiryDate = expiryDate;

            return this;
        }

        public TransportMessageBuilder WithPriority(int priority)
        {
            _transportMessage.Priority = priority;

            return this;
        }

        public TransportMessageBuilder WithEncryption(string encryption)
        {
            _transportMessage.EncryptionAlgorithm = encryption;

            return this;
        }

        public TransportMessageBuilder WithCompression(string compression)
        {
            _transportMessage.CompressionAlgorithm = compression;

            return this;
        }

        public TransportMessageBuilder WithCorrelationId(string correlationId)
        {
            _transportMessage.CorrelationId = correlationId;

            return this;
        }

        public TransportMessageBuilder WithRecipient(IQueue queue)
        {
            return WithRecipient(queue.Uri.ToString());
        }

        private void GuardRecipient()
        {
            if (!HasRecipient && !ShouldSendLocal)
            {
                return;
            }

            throw new InvalidOperationException(Resources.TransportMessageRecipientException);
        }

        public TransportMessageBuilder WithRecipient(Uri uri)
        {
            return WithRecipient(uri.ToString());
        }

        public TransportMessageBuilder WithRecipient(string uri)
        {
            GuardRecipient();
            
            _transportMessage.RecipientInboxWorkQueueUri = uri;

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
            _transportMessage.SenderInboxWorkQueueUri = uri;

            return this;
        }

        public TransportMessageBuilder Local()
        {
            GuardRecipient();
            
            ShouldSendLocal = true;

            return this;
        }

        public TransportMessageBuilder Reply()
        {
            if (!HasTransportMessageReceived || string.IsNullOrEmpty(_transportMessageReceived.SenderInboxWorkQueueUri))
            {
                throw new InvalidOperationException(Resources.SendReplyException);
            }

            GuardRecipient(); 
            
            WithRecipient(_transportMessageReceived.SenderInboxWorkQueueUri);

            return this;
        }
    }
}