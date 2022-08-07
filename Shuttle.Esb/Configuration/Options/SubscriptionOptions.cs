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
        public const string SectionName = "Shuttle:Subscription";

        public SubscribeType SubscribeType { get; set; }
        public string ConnectionStringName { get; set; } = "Subscription";
        public List<string> MessageTypes { get; set; } = new List<string>();
    }
}