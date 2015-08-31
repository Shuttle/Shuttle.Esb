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

		private Thread thread;

		private readonly ILog log;

		public ProcessorThread(string name, IProcessor processor)
		{
			_name = name;
			_processor = processor;

			log = Log.For(this);
		}

		public void Start()
		{
			if (_active)
			{
				return;
			}

			thread = new Thread(Work) {Name = _name};

			thread.SetApartmentState(ApartmentState.MTA);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Normal;

			_active = true;

			thread.Start();

			log.Trace(string.Format(ESBResources.TraceProcessorThreadStarting, thread.ManagedThreadId,
			                        _processor.GetType().FullName));

			while (!thread.IsAlive && _active)
			{
			}

			if (_active)
			{
				log.Trace(string.Format(ESBResources.TraceProcessorThreadActive, thread.ManagedThreadId,
				                        _processor.GetType().FullName));
			}
		}

		public void Stop()
		{
			log.Trace(string.Format(ESBResources.TraceProcessorThreadStopping, thread.ManagedThreadId,
			                        _processor.GetType().FullName));

			_active = false;

			if (thread.IsAlive)
			{
				thread.Join(_threadJoinTimeoutInterval);
			}
		}

		private void Work()
		{
			while (_active)
			{
				log.Verbose(string.Format(ESBResources.VerboseProcessorExecuting, thread.ManagedThreadId,
				                          _processor.GetType().FullName));

				_processor.Execute(this);
			}

			log.Trace(string.Format(ESBResources.TraceProcessorThreadStopped, thread.ManagedThreadId,
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