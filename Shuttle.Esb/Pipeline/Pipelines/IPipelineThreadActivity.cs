using System;

namespace Shuttle.Esb;

public interface IPipelineThreadActivity
{
    void OnThreadWaiting(object sender, ThreadStateEventArgs args);

    void OnThreadWorking(object sender, ThreadStateEventArgs args);
    event EventHandler<ThreadStateEventArgs> ThreadWaiting;
    event EventHandler<ThreadStateEventArgs> ThreadWorking;
}