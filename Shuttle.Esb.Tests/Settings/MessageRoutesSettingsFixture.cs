using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class MessageRoutesSettingsFixture : SettingsFixture
    {
        [Test]
        public void Should_be_able_to_load_the_configuration()
        {
            var settings = GetSettings();

            Assert.IsNotNull(settings);
            Assert.AreEqual(2, settings.MessageRoutes.Length);

            foreach (var messageRouteSettings in settings.MessageRoutes)
            {
                Console.WriteLine(messageRouteSettings.Uri);

                foreach (var specification in messageRouteSettings.Specifications)
                {
                    Console.WriteLine($@"-> {specification.Name} - {specification.Value}");
                }

                Console.WriteLine();
            }
        }
    }
}