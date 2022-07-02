﻿using System;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Tests
{
    public class NullQueueFactory : IQueueFactory
    {
        public NullQueueFactory()
        {
            Scheme = "null-queue";
        }

        public string Scheme { get; }

        public IQueue Create(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));

            return new NullQueue(uri);
        }
    }
}