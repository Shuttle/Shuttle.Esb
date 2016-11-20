using System;

namespace Shuttle.Esb
{
	public class MissingConfigurationAssignmentException : Exception
	{
		public MissingConfigurationAssignmentException(string message) : base(message)
		{
		}
	}
}