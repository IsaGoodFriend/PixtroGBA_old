using System;
using System.Windows.Forms;

namespace Pixtro.Client.Editor
{
	public class LuaCheckbox : CheckBox
	{
		private void DoLuaClick(object sender, EventArgs e)
		{
			var parent = Parent as LuaWinform;
			parent?.DoLuaEvent(Handle);
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			DoLuaClick(this, e);
		}
	}
}
