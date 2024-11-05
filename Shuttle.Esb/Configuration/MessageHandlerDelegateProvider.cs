using System;
using System.Collections.Generic;

namespace Shuttle.Esb;

public class MessageHandlerDelegateProvider : IMessageHandlerDelegateProvider
{
    public MessageHandlerDelegateProvider(IDictionary<Type, MessageHandlerDelegate> delegates)
    {
        Delegates = delegates;
    }

    public IDictionary<Type, MessageHandlerDelegate> Delegates { get; }
}