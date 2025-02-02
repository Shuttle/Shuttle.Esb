using Shuttle.Core.Contract;

namespace Shuttle.Esb.Tests;

public class SimpleCommand : object
{
    public SimpleCommand()
        : this(Guard.AgainstNull(typeof(SimpleCommand).FullName))
    {
    }

    public SimpleCommand(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}