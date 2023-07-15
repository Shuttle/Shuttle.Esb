using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class OperationCompletedEventArgs : EventArgs
    {
        public string Name { get; }
        public object Data { get; }

        public OperationCompletedEventArgs(string name, object data = null)
        {
            Name = Guard.AgainstNullOrEmptyString(name, nameof(name));
            Data = data;
        }
    }
}