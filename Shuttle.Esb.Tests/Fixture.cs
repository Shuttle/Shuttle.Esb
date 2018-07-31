using NUnit.Framework;
using Shuttle.Core.Logging;

namespace Shuttle.Esb.Tests
{
    [SetUpFixture]
    public class Fixture
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            var consoleLog = new ConsoleLog(GetType()) {LogLevel = LogLevel.Trace};


            Log.Assign(consoleLog);
        }
    }
}