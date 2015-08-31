using System;
using System.Threading;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
    public class ThreadActivity : IThreadActivity
    {
        private static readonly TimeSpan _defaultDuration = TimeSpan.FromMilliseconds(250);

        private int _durationIndex;
        private readonly TimeSpan[] _durations;

        public ThreadActivity(TimeSpan[] durationToSleepWhenIdle)
        {
            Guard.AgainstNull(durationToSleepWhenIdle, "durationToSleepWhenIdle");

            _durations = durationToSleepWhenIdle;
            _durationIndex = 0;
        }

        public ThreadActivity(IThreadActivityConfiguration threadActivityConfiguration)
        {
            Guard.AgainstNull(threadActivityConfiguration, "threadActivityConfiguration");

            _durations = threadActivityConfiguration.DurationToSleepWhenIdle;
            _durationIndex = 0;
        }

        private TimeSpan GetSleepTimeSpan()
        {
            if (_durations == null || _durations.Length == 0)
            {
                return _defaultDuration;
            }

            if (_durationIndex >= _durations.Length)
            {
                _durationIndex = _durations.Length - 1;
            }

            return _durations[_durationIndex++];
        }

        public void Waiting(IThreadState state)
        {
        	var ms = (int) GetSleepTimeSpan().TotalMilliseconds;

        	ThreadSleep.While(ms, state);
        }

        public void Working()
        {
            _durationIndex = 0;
        }
    }
}