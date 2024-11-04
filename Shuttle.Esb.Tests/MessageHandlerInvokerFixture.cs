using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Tests;

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
        public int Count { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Replied { get; set; }
    }

    public class MessageHandler : IMessageHandler<Message>
    {
        private readonly IMessageHandlerTracker _messageHandlerTracker;

        public MessageHandler(IMessageHandlerTracker messageHandlerTracker)
        {
            _messageHandlerTracker = Guard.AgainstNull(messageHandlerTracker);
        }

        public async Task ProcessMessageAsync(IHandlerContext<Message> context)
        {
            Console.WriteLine($@"[handled] : name = {context.Message.Name}");

            _messageHandlerTracker.Handled();

            if (context.Message.Replied)
            {
                Assert.That(!context.TransportMessage.MessageReceivedId.Equals(Guid.Empty), "Should have a `MessageReceivedId` value since the message is handled after being sent from a related message.");

                return;
            }

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

    [Test]
    public async Task Should_be_able_to_invoke_handler_async()
    {
        const int count = 5;

        var services = new ServiceCollection();

        services.AddServiceBus(builder =>
        {
            builder.Options.Inbox.ThreadCount = 1;
            builder.Options.Inbox.WorkQueueUri = "memory://configuration/inbox";
            builder.Options.Inbox.DurationToSleepWhenIdle = new()
            {
                TimeSpan.FromMilliseconds(5)
            };
            builder.Options.MessageRoutes.Add(new()
            {
                Uri = "memory://configuration/inbox",
                Specifications = new()
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

            while (messageHandlerTracker.HandledCount < count * 2 && DateTime.Now < timeout)
            {
                Thread.Sleep(25);
            }
        }

        Assert.That(timeout > DateTime.Now, "Timed out before all messages were handled.");
    }
}