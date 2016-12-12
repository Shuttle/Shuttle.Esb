using System;
using NUnit.Framework;

namespace Shuttle.Esb.Tests
{
	[TestFixture]
	public class UriResolverServiceBusSection : ServiceBusSectionFixture
	{
		[Test]
		[TestCase("UriResolver.config")]
		[TestCase("UriResolver-Grouped.config")]
		public void Should_be_able_to_load_the_configuration(string file)
		{
			var section = GetServiceBusSection(file);

			Assert.IsNotNull(section);
			Assert.IsNotNull(section.UriResolver);
			Assert.AreEqual(2, section.UriResolver.Count);

			foreach (UriResolverItemElement uriResolverItemElement in section.UriResolver)
			{
				Console.WriteLine("{0}: {1}", uriResolverItemElement.ResolverUri, uriResolverItemElement.TargetUri);
			}
		}
	}
}