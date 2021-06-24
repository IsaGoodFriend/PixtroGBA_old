using System;

using Pixtro.Emulation.Common;

namespace Pixtro.Client.Common
{
	public interface INamedLuaFunction
	{
		Action Callback { get; }

		Guid Guid { get; }

		MemoryCallbackDelegate MemCallback { get; }

		string Name { get; }
	}
}
