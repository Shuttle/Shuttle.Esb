using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public class PipelineThreadActivity : IPipelineThreadActivity
{
    public event EventHandler<ThreadStateEventArgs>? ThreadWorking;
    public event EventHandler<ThreadStateEventArgs>? ThreadWaiting;

    public void OnThreadWorking(object sender, ThreadStateEventArgs args)
    {
        ThreadWorking?.Invoke(Guard.AgainstNull(sender), Guard.AgainstNull(args));
    }

    public void OnThreadWaiting(object sender, ThreadStateEventArgs args)
    {
        ThreadWaiting?.Invoke(Guard.AgainstNull(sender), Guard.AgainstNull(args));
    }
}