using System;
using System.IO;

namespace Pixtro.Client.Common
{
	public interface IZipWriter : IDisposable
	{
		void WriteItem(string name, Action<Stream> callback);
	}
}
