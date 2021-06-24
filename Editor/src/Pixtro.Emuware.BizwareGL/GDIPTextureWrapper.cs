using System;

namespace Pixtro.Emuware.BizwareGL
{
	public class GDIPTextureWrapper : IDisposable
	{
		public System.Drawing.Bitmap SDBitmap;
		public TextureMinFilter MinFilter = TextureMinFilter.Nearest;
		public TextureMagFilter MagFilter = TextureMagFilter.Nearest;
		public void Dispose()
		{
			if (SDBitmap != null)
			{
				SDBitmap.Dispose();
				SDBitmap = null;
			}
		}
	}
}
