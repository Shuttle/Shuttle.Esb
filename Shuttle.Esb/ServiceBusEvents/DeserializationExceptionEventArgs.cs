using System;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb
{
    public class DeserializationExceptionEventArgs : PipelineEventEventArgs
    {
        public DeserializationExceptionEventArgs(IPipelineEvent pipelineEvent, IBrokerEndpoint workBrokerEndpoint, IBrokerEndpoint errorBrokerEndpoint,
            Exception exception)
            : base(pipelineEvent)
        {
            WorkBrokerEndpoint = workBrokerEndpoint;
            ErrorBrokerEndpoint = errorBrokerEndpoint;
            Exception = exception;
        }

        public IBrokerEndpoint WorkBrokerEndpoint { get; }
        public IBrokerEndpoint ErrorBrokerEndpoint { get; }
        public Exception Exception { get; }
    }
}