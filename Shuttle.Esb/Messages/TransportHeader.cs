using System;

namespace Shuttle.Esb;

[Serializable]
public class TransportHeader
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}