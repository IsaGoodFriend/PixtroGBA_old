using System;
using System.Collections.Generic;
using Pixtro.Emulation.Common;

namespace Pixtro.Emulation.Common
{
	public interface IVirtualPadSchema
	{
		IEnumerable<PadSchema> GetPadSchemas(IEmulator core, Action<string> showMessageBox);
	}
}
