global using static PomCore.Mod;
global using static PomCore.Utils;
global using static PomCore.Logfix;
global using PomCore;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;




namespace PomCore;
/// <summary>
/// This is ManagedPlacedObjects mini-framework, written by henpemaz
/// and ported to Downpour by Thalber. 
/// It is licensed under Creative Commons 0 and is designed
/// to be "stackable" at runtime with multiple instances of POM
/// in multiple mods. It is not recommended to do that anymore, but
/// you still can if you really want to.
/// </summary>
[BepInEx.BepInPlugin("rwmodding.coreorg.pom", "Pom", "2.9")]
public class Mod : BepInEx.BaseUnityPlugin
{
	BepInEx.Configuration.ConfigEntry<bool>? writeTraceConfig;
	/// <inheritdoc/>
	public void OnEnable()
	{
		try
		{
			writeTraceConfig = Config.Bind("main", "writeTrace", false, "Write additional spammy debug lines");
			__writeTrace = writeTraceConfig.Value;
			writeTraceConfig.SettingChanged += (sender, args) =>
			{
				__writeTrace = writeTraceConfig.Value;
			};
			Logfix.__SwitchToBepinexLogger(Logger);
			//instance = this;
			EffExt.Examples.__RegisterExamples();
			Pom.Examples.__RegisterExamples();
		}
		catch (Exception ex)
		{
			LogFatal(ex);
		}
	}
	/// <inheritdoc/>
	public void OnDisable()
	{
		Pom.Pom.DisposeStaticInputHooks();
	}
}
