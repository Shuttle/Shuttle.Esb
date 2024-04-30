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

        public class MessageHandler : IMessageHandler<Message>, IAsyncMessageHandler<Message>
        {
            private readonly IMessageHandlerTracker _messageHandlerTracker;

            public MessageHandler(IMessageHandlerTracker messageHandlerTracker)
            {
                Guard.AgainstNull(messageHandlerTracker, nameof(messageHandlerTracker));

                _messageHandlerTracker = messageHandlerTracker;
            }

            private async Task ProcessMessageAsync(IHandlerContext<Message> context, bool sync)
            {
                Console.WriteLine($@"[handled] : name = {context.Message.Name}");

                _messageHandlerTracker.Handled();

                if (context.Message.Replied)
                {
                    Assert.That(!context.TransportMessage.MessageReceivedId.Equals(Guid.Empty), "Should have a `MessageReceivedId` value since the message is handled after being sent from a related message.");

                    return;
                }

                if (sync)
                {
                    context.Send(new Message
                    {
                        Replied = true,
                        Name = $"replied-{context.Message.Count}"
                    }, builder =>
                    {
                        builder.Reply();
                    });
                }
                else
                {
                    await context.SendAsync(new Message
                    {
                        Replied = true,
                        Name = $"replied-{context.Message.Count}"
                    }, builder =>
                    {
                        builder.Reply();
                    });
                }
            }

            public async Task ProcessMessageAsync(IHandlerContext<Message> context)
            {
                await ProcessMessageAsync(context, false);
            }

            public void ProcessMessage(IHandlerContext<Message> context)
            {
                ProcessMessageAsync(context, true).GetAwaiter().GetResult();
            }
        }

        [Test]
        public void Should_be_able_to_invoke_handler()
        {
            Should_be_able_to_invoke_handler_async(true).GetAwaiter().GetResult();
        }

        [Test]
        public async Task Should_be_able_to_invoke_handler_async()
        {
            await Should_be_able_to_invoke_handler_async(false);
        }

        private async Task Should_be_able_to_invoke_handler_async(bool sync)
        {
            const int count = 5;

            var services = new ServiceCollection();

            services.AddServiceBus(builder =>
            {
                builder.Options.Asynchronous = !sync;
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

            DateTime timeout;

            if (sync)
            {
                using (var serviceBus = serviceProvider.GetRequiredService<IServiceBus>().Start())
                {
                    for (var i = 0; i < count; i++)
                    {
                        serviceBus.Send(new Message
                        {
                            Count = i + 1,
                            Name = $"message - {i + 1}"
                        });
                    }

                    timeout = DateTime.Now.AddSeconds(5);

                    while (messageHandlerTracker.HandledCount < (count * 2) &&
                           DateTime.Now < timeout)
                    {
                        Thread.Sleep(25);
                    }
                }
            }
            else
            {
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

                    timeout = DateTime.Now.AddSeconds(5);

                    while (messageHandlerTracker.HandledCount < (count * 2) &&
                           DateTime.Now < timeout)
                    {
                        Thread.Sleep(25);
                    }
                }
            }

            Assert.That(timeout > DateTime.Now, "Timed out before all messages were");
        }
    }
}