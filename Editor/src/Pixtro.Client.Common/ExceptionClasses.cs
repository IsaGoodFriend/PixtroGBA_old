using System;

namespace Pixtro.Client.Common
{
	public class MoviePlatformMismatchException : InvalidOperationException
	{
		public MoviePlatformMismatchException(string message) : base(message)
		{
		}
	}
}
