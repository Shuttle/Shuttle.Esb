using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public class PipelineThreadActivity : IPipelineThreadActivity
    {
        public event EventHandler<ThreadStateEventArgs> ThreadWorking = delegate
        {
        };

        public event EventHandler<ThreadStateEventArgs> ThreadWaiting = delegate
        {
        };

        public void OnThreadWorking(object sender, ThreadStateEventArgs args)
        {
            Guard.AgainstNull(sender, nameof(sender));
            Guard.AgainstNull(args, nameof(args));

            ThreadWorking.Invoke(sender, args);
        }

        public void OnThreadWaiting(object sender, ThreadStateEventArgs args)
        {
            Guard.AgainstNull(sender, nameof(sender));
            Guard.AgainstNull(args, nameof(args));

            ThreadWaiting.Invoke(sender, args);
        }
    }
}