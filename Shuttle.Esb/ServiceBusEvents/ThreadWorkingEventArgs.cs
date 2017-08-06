using System;

namespace Shuttle.Esb
{
    public class ThreadStateEventArgs : EventArgs
    {
        public ThreadStateEventArgs(Type pipelineType)
        {
            PipelineType = pipelineType;
        }

        public Type PipelineType { get; }
    }
}