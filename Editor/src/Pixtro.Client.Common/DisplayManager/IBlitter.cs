using System.Drawing;

using Pixtro.Emuware.BizwareGL;

namespace Pixtro.Client.Common
{
	/// <summary>
	/// This is an old abstracted rendering class that the OSD system is using to get its work done.
	/// We should probably just use a GuiRenderer (it was designed to do that) although wrapping it with
	/// more information for OSDRendering could be helpful I suppose
	/// </summary>
	public interface IBlitter
	{
		StringRenderer GetFontType(string fontType);
		void DrawString(string s, StringRenderer font, Color color, float x, float y);
		SizeF MeasureString(string s, StringRenderer font);
		Rectangle ClipBounds { get; set; }
	}
}
