using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class OperationEventArgs : EventArgs
    {
        public string Name { get; }
        public object Data { get; }

        public OperationEventArgs(string name, object data = null)
        {
            Name = Guard.AgainstNullOrEmptyString(name, nameof(name));
            Data = data;
        }
    }
}