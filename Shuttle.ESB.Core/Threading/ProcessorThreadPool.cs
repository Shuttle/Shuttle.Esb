using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class ProcessorThreadPool : IProcessorThreadPool
	{
		private readonly string _name;
		private readonly IProcessorFactory _processorFactory;
		private readonly int _threadCount;
		private readonly List<ProcessorThread> _threads = new List<ProcessorThread>();
		private bool _disposed;
		private bool _started;
		private readonly ILog _log;

		public ProcessorThreadPool(string name, int threadCount, IProcessorFactory processorFactory)
		{
			_name = name;
			_threadCount = threadCount;
			_processorFactory = processorFactory;

			_log = Log.For(this);
		}

		public void Pause()
		{
			foreach (var thread in _threads)
			{
				thread.Stop();
			}

			_log.Information(string.Format(ESBResources.ThreadPoolStatusChange, _name, "paused"));
		}

		public void Resume()
		{
			foreach (var thread in _threads)
			{
				thread.Start();
			}

			_log.Information(string.Format(ESBResources.ThreadPoolStatusChange, _name, "resumed"));
		}

		public IProcessorThreadPool Start()
		{
			if (_started)
			{
				return this;
			}

			if (_threadCount < 1)
			{
				throw new ThreadCountZeroException();
			}

			StartThreads();

			_started = true;

			_log.Information(string.Format(ESBResources.ThreadPoolStatusChange, _name, "started"));

			return this;
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		private void StartThreads()
		{
			var i = 0;

			while (i++ < _threadCount)
			{
				var thread = new ProcessorThread(string.Format("{0} / {1}", _name, i), _processorFactory.Create());

				_threads.Add(thread);

				thread.Start();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				foreach (var thread in _threads)
				{
					thread.Deactivate();
				}

				foreach (var thread in _threads)
				{
					thread.Stop();
				}
			}

			_disposed = true;
		}
	}
}