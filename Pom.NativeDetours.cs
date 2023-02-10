using MonoMod.RuntimeDetour;
using UnityEngine;
namespace Pom;

public static partial class Pom
{

	private static bool _stringsdetoured = false;
	/// <summary>
	/// Applies the Input detours required for text input.
	/// Called when any string input reprs are created.
	/// </summary>
	private static void SetupInputDetours()
	{
		if (_stringsdetoured) return;
		_stringsdetoured = true;

		System.Reflection.BindingFlags bindingFlags =
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
		System.Reflection.MethodBase getKeyMethod;
		System.Reflection.MethodBase captureInputMethod;

		getKeyMethod = typeof(Input).GetMethod("GetKey", new Type[] { typeof(string) });
		captureInputMethod = typeof(Pom)
				.GetMethod("CaptureInput", bindingFlags, null, new Type[] { typeof(string) }, null);
		inputDetour_string = new NativeDetour(getKeyMethod, captureInputMethod);

		getKeyMethod = typeof(Input).GetMethod("GetKey", new Type[] { typeof(KeyCode) });
		captureInputMethod = typeof(Pom)
				.GetMethod("CaptureInput", bindingFlags, null, new Type[] { typeof(KeyCode) }, null);
		inputDetour_code = new NativeDetour(getKeyMethod, captureInputMethod);

		On.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate;
	}

	private static void RainWorldGame_RawUpdate(
		On.RainWorldGame.orig_RawUpdate orig,
		RainWorldGame self,
		float dt)
	{
		orig(self, dt);
		if (self.devUI == null)
		{
			ManagedStringControl.activeStringControl = null;     // remove string control focus when dev tools are closed
		}
	}


#pragma warning disable IDE0051
	private static NativeDetour? inputDetour_string;
	private static NativeDetour? inputDetour_code;

	private static void UndoInputDetours()
	{
		inputDetour_string?.Undo();
		inputDetour_code?.Undo();
	}

	private static bool CaptureInput(string key)
	{
		key = key.ToUpper();
		KeyCode code = (KeyCode)Enum.Parse(typeof(KeyCode), key);
		return CaptureInput(code);
	}

	private static bool CaptureInput(KeyCode code)
	{
		bool res;

		if (ManagedStringControl.activeStringControl == null)
		{
			res = Orig_GetKey(code);
		}
		else
		{
			if (code == KeyCode.Escape)
			{
				res = Orig_GetKey(code);
			}
			else
			{
				res = false;
			}
		}

		return res;
	}

	private static bool Orig_GetKey(KeyCode code)
	{
		inputDetour_code?.Undo();
		bool res = Input.GetKey(code);
		inputDetour_code?.Apply();
		return res;
	}

}
