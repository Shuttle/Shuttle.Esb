using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Transactions;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ServiceBusFixture
    {
        [Test]
        public void Should_be_able_to_handle_expired_message()
        {
            var handlerInvoker = new FakeMessageHandlerInvoker();
            var endpoint = new FakeBrokerEndpoint(2);

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddTransactionScope(options =>
            {
                options.Disable();
            });
            services.AddSingleton<IMessageHandlerInvoker>(handlerInvoker);
            services.AddServiceBus(builder =>
            {
                builder.Configure(configuration =>
                {
                    configuration.Inbox = new InboxConfiguration
                    {
                        BrokerEndpoint = endpoint,
                        ErrorBrokerEndpoint = endpoint,
                        ThreadCount = 1
                    };
                });
            });

            using (var bus = services.BuildServiceProvider().GetRequiredService<IServiceBus>().Start())
            {
                var timeout = DateTime.Now.AddSeconds(1);

                while (endpoint.MessageCount < 2 && DateTime.Now < timeout)
                {
                    Thread.Sleep(5);
                }
            }

            Assert.AreEqual(1, handlerInvoker.GetInvokeCount("SimpleCommand"),
                "FakeHandlerInvoker was not invoked exactly once.");
            Assert.AreEqual(2, endpoint.MessageCount, "FakeBrokerEndpoint was not invoked exactly twice.");
        }
    }
}