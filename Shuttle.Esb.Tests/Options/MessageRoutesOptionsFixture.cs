using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class MessageRoutesOptionsFixture : OptionsFixture
    {
        [Test]
        public void Should_be_able_to_load_the_configuration()
        {
            var options = GetOptions();

            Assert.IsNotNull(options);
            Assert.AreEqual(2, options.MessageRoutes.Count);

            foreach (var messageRouteOptions in options.MessageRoutes)
            {
                Console.WriteLine(messageRouteOptions.Uri);

                foreach (var specification in messageRouteOptions.Specifications)
                {
                    Console.WriteLine($@"-> {specification.Name} - {specification.Value}");
                }

                Console.WriteLine();
            }
        }
    }
}