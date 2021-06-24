using Pixtro.Emulation.Common;

namespace Pixtro.Emulation.Cores.Nintendo.GBA
{
	public partial class MGBAHawk : IInputPollable
	{
		public int LagCount { get; set; }
		public bool IsLagFrame { get; set; }

		[FeatureNotImplemented]
		public IInputCallbackSystem InputCallbacks { get; private set; }
	}
}
