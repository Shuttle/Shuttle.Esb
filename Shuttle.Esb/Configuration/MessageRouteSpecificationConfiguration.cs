namespace Shuttle.Esb
{
    public class MessageRouteSpecificationConfiguration
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public MessageRouteSpecificationConfiguration(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}