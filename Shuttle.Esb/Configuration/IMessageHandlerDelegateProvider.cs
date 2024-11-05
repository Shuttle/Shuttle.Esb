using System.Collections.Generic;
using System;

namespace Shuttle.Esb;

public interface IMessageHandlerDelegateProvider
{
    IDictionary<Type, MessageHandlerDelegate> Delegates { get; }
}