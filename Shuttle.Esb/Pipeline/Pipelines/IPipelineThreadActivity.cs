using System;

namespace Shuttle.Esb
{
    public interface IPipelineThreadActivity
    {
        event EventHandler<ThreadStateEventArgs> ThreadWorking;
        event EventHandler<ThreadStateEventArgs> ThreadWaiting;

        void OnThreadWorking(object sender, ThreadStateEventArgs args);
        void OnThreadWaiting(object sender, ThreadStateEventArgs args);
    }
}