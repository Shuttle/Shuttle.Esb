using NUnit.Framework;

namespace Shuttle.ESB.Core.Tests
{
	[TestFixture]
	public class TypeListMessageRouteSpecificationTest
	{
		[Test]
		public void Should_be_able_to_get_types_from_given_valid_value_string()
		{
			new TypeListMessageRouteSpecification(
				"Shuttle.ESB.Core.Tests.SimpleCommand, Shuttle.ESB.Core.Tests;" +
				"Shuttle.ESB.Core.Tests.SimpleEvent, Shuttle.ESB.Core.Tests");
		}

		[Test]
		public void Should_fail_when_given_a_type_that_cannot_be_determined()
		{
			Assert.Throws<MessageRouteSpecificationException>(() => new TypeListMessageRouteSpecification("bogus"));
		}
	}
}