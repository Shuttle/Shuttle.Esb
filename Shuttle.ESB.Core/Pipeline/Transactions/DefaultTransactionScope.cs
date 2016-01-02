using System;
using System.Threading;
using System.Transactions;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.Core
{
	public class DefaultTransactionScope : ITransactionScope
	{
		private readonly bool _ignore;
        private readonly string _name;
        private readonly TransactionScope _scope;

        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadUncommitted;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

		private readonly ILog _log;

        public DefaultTransactionScope()
            : this(Guid.NewGuid().ToString("n"), DefaultIsolationLevel, TimeSpan.FromMinutes(15))
        {
        }

        public DefaultTransactionScope(IsolationLevel isolationLevel, TimeSpan timeout)
            : this(Guid.NewGuid().ToString("n"), isolationLevel, timeout)
        {
        }

        public DefaultTransactionScope(string name)
            : this(name, DefaultIsolationLevel, DefaultTimeout)
        {
        }

        public DefaultTransactionScope(string name, IsolationLevel isolationLevel, TimeSpan timeout)
        {
            _name = name;

        	_log = Log.For(this);

        	_ignore = Transaction.Current != null;

			if (_ignore)
			{
                if (_log.IsVerboseEnabled)
                {
                    _log.Verbose(string.Format(ESBResources.VerboseTransactionScopeAmbient, name,
                                              Thread.CurrentThread.ManagedThreadId));
                }

			    return;
			}

        	_scope = new TransactionScope(TransactionScopeOption.RequiresNew,
				                             new TransactionOptions
				                             	{
				                             		IsolationLevel = isolationLevel,
				                             		Timeout = timeout
				                             	});

            if (_log.IsVerboseEnabled)
            {
                _log.Verbose(string.Format(ESBResources.VerboseTransactionScopeCreated, name, isolationLevel, timeout,
                                          Thread.CurrentThread.ManagedThreadId));
            }
        }

        public void Dispose()
        {
            if (_scope == null)
            {
                return;
            }

        	try
            {
                _scope.Dispose();
			}
            catch 
            {
                // _ignore --- may be bug in transaction _scope: http://connect.microsoft.com/VisualStudio/feedback/details/449469/transactedconnectionpool-bug-in-vista-server-2008-sp2#details
            }
		}

        public void Complete()
        {
			if (_ignore)
			{
                if (_log.IsVerboseEnabled)
                {
                    _log.Verbose(string.Format(ESBResources.VerboseTransactionScopeAmbientCompleted, _name,
                                              Thread.CurrentThread.ManagedThreadId));
                }

			    return;
			}

			if (_scope == null)
            {
                return;
            }

            _scope.Complete();

            if (_log.IsVerboseEnabled)
            {
                _log.Verbose(string.Format(ESBResources.VerboseTransactionScopeCompleted, _name,
                                          Thread.CurrentThread.ManagedThreadId));
            }
        }
    }
}