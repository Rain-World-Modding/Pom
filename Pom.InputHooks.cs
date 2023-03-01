using MonoMod.RuntimeDetour;
using UnityEngine;
namespace Pom;

public static partial class Pom
{
	private static List<Hook> __inputHooks = new();

	private static void ApplyInputHooks()
	{
		Type inputType = typeof(Input);
		Type[] stringArg = new[] { typeof(string) }, keyCodeArg = new[] { typeof(KeyCode) };
		__inputHooks.Add(new(inputType.GetMethod(nameof(Input.GetKey), stringArg), InputGetKey_string));
		__inputHooks.Add(new(inputType.GetMethod(nameof(Input.GetKey), keyCodeArg), InputGetKey_KeyCode));
		__inputHooks.Add(new(inputType.GetMethod(nameof(Input.GetKeyUp), stringArg), InputGetKey_string));
		__inputHooks.Add(new(inputType.GetMethod(nameof(Input.GetKeyUp), keyCodeArg), InputGetKey_KeyCode));
		__inputHooks.Add(new(inputType.GetMethod(nameof(Input.GetKeyDown), stringArg), InputGetKey_string));
		__inputHooks.Add(new(inputType.GetMethod(nameof(Input.GetKeyDown), keyCodeArg), InputGetKey_KeyCode));
		On.RainWorldGame.RawUpdate += RainWorldGameRawUpdate;
	}

	internal static void UndoInputHooks()
	{
		if (__inputHooks != null)
		{
			for (var i = 0; i < __inputHooks.Count; i++)
				__inputHooks[i].Dispose();
			__inputHooks.Clear();
		}
		On.RainWorldGame.RawUpdate -= RainWorldGameRawUpdate;
	}

	internal static void DisposeStaticInputHooks()
	{
		ManagedStringControl.activeStringControl = null;
		__inputHooks = null!;
	}

	private static void RainWorldGameRawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
	{
		orig(self, dt);
		if (self.devUI == null)
			ManagedStringControl.activeStringControl = null; // Remove string control focus when dev tools are closed.
	}

	private static bool InputGetKey_string(Func<string, bool> orig, string name)
	{
		var res = orig(name);
		if (ManagedStringControl.activeStringControl != null && name.ToLower() != "escape")
			res = false;
		return res;
	}

	private static bool InputGetKey_KeyCode(Func<KeyCode, bool> orig, KeyCode key)
	{
		var res = orig(key);
		if (ManagedStringControl.activeStringControl != null && key != KeyCode.Escape)
			res = false;
		return res;
	}
}
