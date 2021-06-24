using Pixtro.Client.Common;
using Pixtro.Emulation.Common;

namespace Pixtro.Client.Editor
{
	public interface IMainFormForConfig : IDialogParent
	{
		/// <remarks>only referenced from <see cref="GenericCoreConfig"/></remarks>
		IEmulator Emulator { get; }

		IMovieSession MovieSession { get; }

		void AddOnScreenMessage(string message);

		void PutCoreSettings(object o);

		void PutCoreSyncSettings(object o);
	}
}
