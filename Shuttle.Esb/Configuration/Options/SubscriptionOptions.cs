using System;
using System.Collections.Generic;

namespace Shuttle.Esb;

public enum SubscribeType
{
    Normal = 0,
    Ensure = 1,
    Ignore = 2
}

public class SubscriptionOptions
{
    public List<string> MessageTypes { get; set; } = new();
    public SubscribeType SubscribeType { get; set; } = SubscribeType.Normal;
}