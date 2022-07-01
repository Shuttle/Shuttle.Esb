using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb
{
    public static class BrokerEndpointExtensions
    {
        public static bool AttemptCreate(this IBrokerEndpoint brokerEndpoint)
        {
            var operation = brokerEndpoint as ICreateBrokerEndpoint;

            if (operation == null)
            {
                return false;
            }

            operation.Create();

            return true;
        }

        public static void Create(this IBrokerEndpoint brokerEndpoint)
        {
            Guard.AgainstNull(brokerEndpoint, nameof(brokerEndpoint));

            var operation = brokerEndpoint as ICreateBrokerEndpoint;

            if (operation == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NotImplementedOnBrokerEndpoint,
                    brokerEndpoint.GetType().FullName, "ICreateBrokerEndpoint"));
            }

            operation.Create();
        }

        public static bool AttemptDrop(this IBrokerEndpoint brokerEndpoint)
        {
            var operation = brokerEndpoint as IDeleteBrokerEndpoint;

            if (operation == null)
            {
                return false;
            }

            operation.Drop();

            return true;
        }

        public static void Delete(this IBrokerEndpoint brokerEndpoint)
        {
            Guard.AgainstNull(brokerEndpoint, nameof(brokerEndpoint));

            var operation = brokerEndpoint as IDeleteBrokerEndpoint;

            if (operation == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NotImplementedOnBrokerEndpoint,
                    brokerEndpoint.GetType().FullName, "DeleteBrokerEndpoint"));
            }

            operation.Drop();
        }

        public static bool TryPurge(this IBrokerEndpoint brokerEndpoint)
        {
            var operation = brokerEndpoint as IPurgeBrokerEndpoint;

            if (operation == null)
            {
                return false;
            }

            operation.Purge();

            return true;
        }

        public static void Purge(this IBrokerEndpoint brokerEndpoint)
        {
            Guard.AgainstNull(brokerEndpoint, nameof(brokerEndpoint));

            var operation = brokerEndpoint as IPurgeBrokerEndpoint;

            if (operation == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NotImplementedOnBrokerEndpoint,
                    brokerEndpoint.GetType().FullName, "IPurgeBrokerEndpoint"));
            }

            operation.Purge();
        }
    }
}