using System;
using System.Windows.Forms;

namespace Pixtro.Client.Editor
{
	internal class LuaButton : Button
	{
		protected override void OnClick(EventArgs e)
		{
			(Parent as LuaWinform)?.DoLuaEvent(Handle);
			base.OnClick(e);
		}
	}
}
