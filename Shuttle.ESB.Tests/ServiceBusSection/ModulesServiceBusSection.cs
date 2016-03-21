using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
	[TestFixture]
	public class ModulesServiceBusSection : ServiceBusSectionFixture
	{
		[Test]
		[TestCase("Modules.config")]
		[TestCase("Modules-Grouped.config")]
		public void Should_be_able_to_load_the_configuration(string file)
		{
			var section = GetServiceBusSection(file);

			Assert.IsNotNull(section);
			Assert.IsNotNull(section.Modules);
			Assert.AreEqual(2, section.Modules.Count);

			foreach (ModuleElement moduleElement in section.Modules)
			{
				Console.WriteLine(moduleElement.Type);
			}
		}

		[Test]
		[TestCase("Empty.config")]
		public void Should_be_able_to_handle_missing_element(string file)
		{
			var section = GetServiceBusSection(file);

			Assert.IsNotNull(section);
			Assert.IsEmpty(section.Modules);
		}
	}
}