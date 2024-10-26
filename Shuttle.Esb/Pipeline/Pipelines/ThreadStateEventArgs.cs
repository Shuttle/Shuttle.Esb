using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb;

public class ThreadStateEventArgs : EventArgs
{
    public ThreadStateEventArgs(IPipeline pipeline)
    {
        Pipeline = pipeline;
    }

    public IPipeline Pipeline { get; }
}