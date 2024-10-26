namespace Shuttle.Esb;

public class MessageRouteSpecificationConfiguration
{
    public MessageRouteSpecificationConfiguration(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public string Value { get; }
}