using System;

namespace Shuttle.Esb
{
    [Serializable]
    public class TransportHeader
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}