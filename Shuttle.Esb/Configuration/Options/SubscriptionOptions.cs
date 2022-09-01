using System;
using System.Collections.Generic;

namespace Shuttle.Esb
{
    public enum SubscribeType
    {
        Normal = 0,
        Ensure = 1,
        Ignore = 2
    }

    public class SubscriptionOptions
    {
        public SubscribeType SubscribeType { get; set; } = SubscribeType.Normal;
        public string ConnectionStringName { get; set; } = "Subscription";
        public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromMinutes(5);
        public List<string> MessageTypes { get; set; } = new List<string>();
    }
}