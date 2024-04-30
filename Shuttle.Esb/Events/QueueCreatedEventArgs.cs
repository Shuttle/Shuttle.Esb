using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class QueueEventArgs : EventArgs
    {
        public QueueEventArgs(IQueue queue)
        {
            Queue = Guard.AgainstNull(queue, nameof(queue));
        }

        public IQueue Queue { get; }
    }
}