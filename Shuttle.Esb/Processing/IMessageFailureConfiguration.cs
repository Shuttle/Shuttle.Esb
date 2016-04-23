using System;

namespace Shuttle.Esb
{
	public interface IMessageFailureConfiguration
	{
		int MaximumFailureCount { get; set; }
		TimeSpan[] DurationToIgnoreOnFailure { get; set; }
	}
}