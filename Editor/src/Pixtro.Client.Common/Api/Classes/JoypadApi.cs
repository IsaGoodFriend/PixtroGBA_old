using System;
using System.Collections.Generic;

using Pixtro.Emulation.Common;

namespace Pixtro.Client.Common
{
	public sealed class JoypadApi : IJoypadApi
	{
		private readonly InputManager _inputManager;


		private readonly Action<string> LogCallback;

		public JoypadApi(Action<string> logCallback, InputManager inputManager)
		{
			LogCallback = logCallback;
			_inputManager = inputManager;
		}

		public IDictionary<string, object> Get(int? controller = null)
		{
			return _inputManager.AutofireStickyXorAdapter.ToDictionary(controller);
		}

		public IDictionary<string, object> GetWithMovie(int? controller = null)
		{
			return _inputManager.ControllerOutput.ToDictionary(controller);
		}

		public IDictionary<string, object> GetImmediate(int? controller = null)
		{
			return _inputManager.ActiveController.ToDictionary(controller);
		}

		public void SetFromMnemonicStr(string inputLogEntry)
		{
		}

		public void Set(IDictionary<string, bool> buttons, int? controller = null)
		{
			// If a controller is specified, we need to iterate over unique button names. If not, we iterate over
			// ALL button names with P{controller} prefixes
			foreach (var button in _inputManager.ActiveController.ToBoolButtonNameList(controller))
			{
				Set(button, buttons.TryGetValue(button, out var state) ? state : (bool?) null, controller);
			}
		}

		public void Set(string button, bool? state = null, int? controller = null)
		{
			try
			{
				var buttonToSet = controller == null ? button : $"P{controller} {button}";
				if (state == null) _inputManager.ButtonOverrideAdapter.UnSet(buttonToSet);
				else _inputManager.ButtonOverrideAdapter.SetButton(buttonToSet, state.Value);
				
				//"Overrides" is a gross line of code in that flushes overrides into the current controller.
				//That's not really the way it was meant to work which was that it should pull all its values through the filters before ever using them.
				//Of course the code that does that is in the main loop and the lua API wouldnt know how to do it.
				//I regret the whole hotkey filter chain OOP soup approach. Anyway, the code that

				//in a crude, CRUDE, *CRUDE* approximation of what the main loop does, we need to pull the physical input again before it's freshly overridded
				//but really, everything the main loop does needs to be done here again.
				//I'm not doing that now.
				_inputManager.ActiveController.LatchFromPhysical(_inputManager.ControllerInputCoalescer);

				//and here's where the overrides managed by this API are pushed in
				_inputManager.ActiveController.Overrides(_inputManager.ButtonOverrideAdapter);
			}
			catch
			{
				// ignored
			}
		}

		public void SetAnalog(IDictionary<string, int?> controls, object controller = null)
		{
			foreach (var kvp in controls) SetAnalog(kvp.Key, kvp.Value, controller);
		}

		public void SetAnalog(string control, int? value = null, object controller = null)
		{
			try
			{
				_inputManager.StickyXorAdapter.SetAxis(controller == null ? control : $"P{controller} {control}", value);
			}
			catch
			{
				// ignored
			}
		}
	}
}
