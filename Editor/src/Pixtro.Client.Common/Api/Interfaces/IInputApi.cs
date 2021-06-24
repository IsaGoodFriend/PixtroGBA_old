using System.Collections.Generic;

namespace Pixtro.Client.Common
{
	public interface IInputApi : IExternalApi
	{
		Dictionary<string, bool> Get();
		Dictionary<string, object> GetMouse();
	}
}
