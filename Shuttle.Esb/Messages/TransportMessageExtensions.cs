using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public static class TransportMessageExtensions
{
    public static void AcceptInvariants(this TransportMessage transportMessage)
    {
        Guard.AgainstNull(transportMessage.MessageId);
        Guard.AgainstNullOrEmptyString(transportMessage.PrincipalIdentityName);
        Guard.AgainstNullOrEmptyString(transportMessage.MessageType);
        Guard.AgainstNullOrEmptyString(transportMessage.AssemblyQualifiedName);
    }

    public static bool CompressionEnabled(this TransportMessage transportMessage)
    {
        return !string.IsNullOrEmpty(transportMessage.CompressionAlgorithm)
               &&
               !transportMessage.CompressionAlgorithm.Equals("none", StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool Contains(this IEnumerable<TransportHeader> headers, string key)
    {
        return headers.Any(header => header.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
    }

    public static bool EncryptionEnabled(this TransportMessage transportMessage)
    {
        return !string.IsNullOrEmpty(transportMessage.EncryptionAlgorithm)
               &&
               !transportMessage.EncryptionAlgorithm.Equals("none", StringComparison.InvariantCultureIgnoreCase);
    }

    public static string GetHeaderValue(this List<TransportHeader> headers, string key)
    {
        var header = headers.FirstOrDefault(candidate => candidate.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));

        return header == null ? string.Empty : header.Value;
    }

    public static bool HasExpired(this TransportMessage transportMessage)
    {
        return transportMessage.ExpiryDate.ToUniversalTime() < DateTimeOffset.UtcNow;
    }

    public static bool HasExpiryDate(this TransportMessage transportMessage)
    {
        return transportMessage.ExpiryDate < DateTimeOffset.MaxValue;
    }

    public static bool HasPriority(this TransportMessage transportMessage)
    {
        return transportMessage.Priority != 0;
    }

    public static bool HasSenderInboxWorkQueueUri(this TransportMessage transportMessage)
    {
        return !string.IsNullOrEmpty(transportMessage.SenderInboxWorkQueueUri);
    }

    public static bool IsIgnoring(this TransportMessage transportMessage)
    {
        return DateTimeOffset.UtcNow < transportMessage.IgnoreTillDate.ToUniversalTime();
    }

    public static void Merge(this List<TransportHeader> merge, IEnumerable<TransportHeader> headers)
    {
        foreach (var header in headers.Where(header => !merge.Contains(header.Key)))
        {
            merge.Add(new()
            {
                Key = header.Key,
                Value = header.Value
            });
        }
    }

    public static void RegisterFailure(this TransportMessage transportMessage, string message)
    {
        transportMessage.RegisterFailure(message, TimeSpan.FromMilliseconds(0));
    }

    public static void RegisterFailure(this TransportMessage transportMessage, string message, TimeSpan timeSpanToIgnore)
    {
        Guard.AgainstNullOrEmptyString(message);

        transportMessage.FailureMessages.Add($"[{DateTimeOffset.UtcNow:O}] : {message}");
        transportMessage.IgnoreTillDate = DateTimeOffset.UtcNow.Add(timeSpanToIgnore);
    }

    public static void SetHeaderValue(this List<TransportHeader> headers, string key, string value)
    {
        var header = Guard.AgainstNull(headers).FirstOrDefault(candidate => candidate.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));

        if (header != null)
        {
            header.Value = value;
        }
        else
        {
            headers.Add(new()
            {
                Key = key,
                Value = value
            });
        }
    }

    public static void StopIgnoring(this TransportMessage transportMessage)
    {
        transportMessage.IgnoreTillDate = DateTimeOffset.MinValue;
    }
}