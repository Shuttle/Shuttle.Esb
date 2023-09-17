using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class MessageHandlerInvokerFixture
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
            public int Count { get; set; }
            public bool Replied { get; set; }
        }

        public class AsyncMessageHandler : IAsyncMessageHandler<Message>
        {
            private readonly IMessageHandlerTracker _messageHandlerTracker;

            public AsyncMessageHandler(IMessageHandlerTracker messageHandlerTracker)
            {
                Guard.AgainstNull(messageHandlerTracker, nameof(messageHandlerTracker));

                _messageHandlerTracker = messageHandlerTracker;
            }

            public async Task ProcessMessage(IHandlerContext<Message> context)
            {
                Console.WriteLine($@"[handled] : name = {context.Message.Name}");

                _messageHandlerTracker.Handled();

                if (context.Message.Replied)
                {
                    Assert.That(!context.TransportMessage.MessageReceivedId.Equals(Guid.Empty), "Should have a `MessageReceivedId` value since the message is handled after being sent from a related message.");

                    return;
                }

                await context.Send(new Message
                {
                    Replied = true,
                    Name = $"replied-{context.Message.Count}"
                }, builder =>
                {
                    builder.Reply();
                });
            }
        }

        [Test]
        public async Task Should_be_able_to_invoke_handler()
        {
            const int count = 5;

            var services = new ServiceCollection();

            services.AddServiceBus(builder =>
            {
                builder.Options.Inbox.ThreadCount = 1;
                builder.Options.Inbox.WorkQueueUri = "memory://configuration/inbox";
                builder.Options.Inbox.DurationToSleepWhenIdle = new List<TimeSpan>
                {
                    TimeSpan.FromMilliseconds(5)
                };
                builder.Options.MessageRoutes.Add(new MessageRouteOptions
                {
                    Uri = "memory://configuration/inbox",
                    Specifications = new List<MessageRouteOptions.SpecificationOptions>
                    {
                        new()
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

            await using (var serviceBus = await serviceProvider.GetRequiredService<IServiceBus>().StartAsync().ConfigureAwait(false))
            {
                for (var i = 0; i < count; i++)
                {
                    await serviceBus.SendAsync(new Message
                    {
                        Count = i + 1,
                        Name = $"message - {i + 1}"
                    });
                }

                var timeout = DateTime.Now.AddSeconds(5);

                while (messageHandlerTracker.HandledCount < (count * 2) &&
                       DateTime.Now < timeout)
                {
                    Thread.Sleep(25);
                }

                Assert.That(timeout > DateTime.Now);
            }
        }
    }
}