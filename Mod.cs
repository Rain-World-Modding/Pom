// This is ManagedPlacedObjects mini-framework, written by henpemaz
// and ported to Downpour by Thalber. 
// It is licensed under Creative Commons 0 and is designed
// to be "stackable" at runtime with multiple instances of POM
// in multiple mods. It is not recommended to do that anymore, but
// you still can if you really want to.
global using static Pom.Mod;

namespace Pom;

[BepInEx.BepInPlugin("rwmodding.coreorg.pom", "Pom", "2.7")]
public class Mod : BepInEx.BaseUnityPlugin
{
	internal static Mod instance = null!;
	public static BepInEx.Logging.ManualLogSource plog => instance.Logger;
	public void OnEnable()
	{
		instance = this;
		Examples.RegisterExamples();
	}

	public void OnDisable()
	{
		Pom.DisposeStaticInputHooks();
		instance = null!;
	}
}
