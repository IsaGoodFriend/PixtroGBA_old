using System.Drawing;

using Pixtro.Emulation.Common;
using Pixtro.Emulation.Cores.Nintendo.GBA;

namespace Pixtro.Client.Editor.CoreExtensions
{
	public static class CoreExtensions
	{
		public static Bitmap Icon(this IEmulator core)
		{
			var attributes = core.Attributes();

			if (attributes is not PortedCoreAttribute)
			{
				return Properties.Resources.CorpHawkSmall;
			}

			return core switch
			{
				MGBAHawk => Properties.Resources.Mgba,
				_ => null
			};
		}

		public static string GetSystemDisplayName(this IEmulator emulator) => emulator switch
		{
			NullEmulator => string.Empty,
#if false
			IGameboyCommon gb when gb.IsCGBMode() => EmulatorExtensions.SystemIDToDisplayName("GBC"),
#endif
			_ => EmulatorExtensions.SystemIDToDisplayName(emulator.SystemId)
		};
	}
}
