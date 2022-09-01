using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class ThreadStateEventArgs : EventArgs
    {
        public IPipeline Pipeline { get; }

        public ThreadStateEventArgs(IPipeline pipeline)
        {
            Pipeline = pipeline;
        }
    }
}