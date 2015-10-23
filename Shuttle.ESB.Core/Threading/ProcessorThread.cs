using System.Threading;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	internal class ProcessorThread : IThreadState
	{
		private readonly string _name;
		private readonly IProcessor _processor;
		private volatile bool _active;

		private readonly int _threadJoinTimeoutInterval =
			ConfigurationItem<int>.ReadSetting("ThreadJoinTimeoutInterval", 1000).GetValue();

		private Thread _thread;

		private readonly ILog _log;

		public ProcessorThread(string name, IProcessor processor)
		{
			_name = name;
			_processor = processor;

			_log = Log.For(this);
		}

		public void Start()
		{
			if (_active)
			{
				return;
			}

			_thread = new Thread(Work) {Name = _name};

			_thread.SetApartmentState(ApartmentState.MTA);
			_thread.IsBackground = true;
			_thread.Priority = ThreadPriority.Normal;

			_active = true;

			_thread.Start();

			_log.Trace(string.Format(ESBResources.TraceProcessorThreadStarting, _thread.ManagedThreadId,
			                        _processor.GetType().FullName));

			while (!_thread.IsAlive && _active)
			{
			}

			if (_active)
			{
				_log.Trace(string.Format(ESBResources.TraceProcessorThreadActive, _thread.ManagedThreadId,
				                        _processor.GetType().FullName));
			}
		}

		public void Stop()
		{
			_log.Trace(string.Format(ESBResources.TraceProcessorThreadStopping, _thread.ManagedThreadId,
			                        _processor.GetType().FullName));

			_active = false;

			if (_thread.IsAlive)
			{
				_thread.Join(_threadJoinTimeoutInterval);
			}
		}

		private void Work()
		{
			while (_active)
			{
				_log.Verbose(string.Format(ESBResources.VerboseProcessorExecuting, _thread.ManagedThreadId,
				                          _processor.GetType().FullName));

				_processor.Execute(this);
			}

			_log.Trace(string.Format(ESBResources.TraceProcessorThreadStopped, _thread.ManagedThreadId,
			                        _processor.GetType().FullName));
		}

		public bool Active
		{
			get { return _active; }
		}

		internal void Deactivate()
		{
			_active = false;
		}
	}
}