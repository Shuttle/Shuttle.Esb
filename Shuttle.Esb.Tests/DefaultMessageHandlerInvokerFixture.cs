﻿using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class DefaultMessageHandlerInvokerFixture
    {
        public interface IMessageHandlerTracker
        {
            int HandledCount { get; }
            void Handled();
        }

        public class MessageHandlerTracker : IMessageHandlerTracker
        {
            public int HandledCount { get; private set; }

            public void Handled()
            {
                HandledCount++;
            }
        }

        public class Message
        {
            public string Name { get; set; }
        }

        public class MessageHandler : IMessageHandler<Message>
        {
            private readonly IMessageHandlerTracker _messageHandlerTracker;

            public MessageHandler(IMessageHandlerTracker messageHandlerTracker)
            {
                Guard.AgainstNull(messageHandlerTracker, nameof(messageHandlerTracker));

                _messageHandlerTracker = messageHandlerTracker;
            }

            public void ProcessMessage(IHandlerContext<Message> context)
            {
                Console.WriteLine($@"[handled] : name = {context.Message.Name}");

                _messageHandlerTracker.Handled();
            }
        }

        [Test]
        public void Should_be_able_to_invoke_handler()
        {
            const int count = 5;

            var services = new ServiceCollection();

            services.AddServiceBus(builder =>
            {
                builder.Options.Inbox.ThreadCount = 1;
                builder.Options.Inbox.WorkQueueUri = "memory://inbox";
                builder.Options.Inbox.DurationToSleepWhenIdle = new List<TimeSpan>
                {
                    TimeSpan.FromMilliseconds(5)
                };
                builder.Options.MessageRoutes.Add(new MessageRouteOptions
                {
                    Uri = "memory://inbox",
                    Specifications = new List<MessageRouteOptions.SpecificationOptions>
                    {
                        new MessageRouteOptions.SpecificationOptions
                        {
                            Name = "StartsWith",
                            Value = "Shuttle"
                        }
                    }
                });
            });

            services.AddSingleton<IQueueFactory, MemoryQueueFactory>();
            services.AddSingleton<IMessageHandlerTracker, MessageHandlerTracker>();

            var serviceProvider = services.BuildServiceProvider();

            var messageHandlerTracker = serviceProvider.GetRequiredService<IMessageHandlerTracker>();

            using (var serviceBus = serviceProvider.GetRequiredService<IServiceBus>().Start())
            {
                for (var i = 0; i < count; i++)
                {
                    serviceBus.Send(new Message
                    {
                        Name = $"message - {i + 1}"
                    });
                }

                var timeout = DateTime.Now.AddSeconds(5);

                while (messageHandlerTracker.HandledCount < count &&
                       DateTime.Now < timeout)
                {
                    Thread.Sleep(25);
                }

                Assert.That(timeout > DateTime.Now);
            }
        }
    }
}