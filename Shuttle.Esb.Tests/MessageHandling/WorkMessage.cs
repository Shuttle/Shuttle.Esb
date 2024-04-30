using System;

namespace Shuttle.Esb.Tests.MessageHandling;

public class WorkMessage
{
    public Guid Guid { get; set; } = Guid.NewGuid();
}