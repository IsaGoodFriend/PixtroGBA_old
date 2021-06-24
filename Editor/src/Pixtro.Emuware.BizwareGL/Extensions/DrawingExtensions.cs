using System.Drawing;

namespace Pixtro.Emuware.BizwareGL.DrawingExtensions
{
	public static class DrawingExtensions
	{
		public static void Deconstruct(this Size size, out int width, out int height)
		{
			width = size.Width;
			height = size.Height;
		}
	}
}
