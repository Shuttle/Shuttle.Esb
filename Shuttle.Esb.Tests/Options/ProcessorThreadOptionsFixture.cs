using System;
using System.Threading;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
    [TestFixture]
    public class ProcessorThreadOptionsFixture : OptionsFixture
    {
        [Test]
        public void Should_be_able_to_load_a_valid_configuration()
        {
            var options = GetOptions();

            Assert.IsNotNull(options);
            Assert.That(options.ProcessorThread.IsBackground, Is.False);
            Assert.That(options.ProcessorThread.JoinTimeout, Is.EqualTo(TimeSpan.FromSeconds(15)));
            Assert.That(options.ProcessorThread.Priority, Is.EqualTo(ThreadPriority.Lowest));
        }
    }
}