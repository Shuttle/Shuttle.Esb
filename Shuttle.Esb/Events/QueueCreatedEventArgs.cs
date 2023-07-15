using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class QueueCreatedEventArgs : EventArgs
    {
        public QueueCreatedEventArgs(IQueue queue)
        {
            Queue = Guard.AgainstNull(queue, nameof(queue));
        }

        public IQueue Queue { get; private set; }
    }
}