global using PomCore;
global using static PomCore.Logfix;
global using static PomCore.Utils;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;

#pragma warning disable CS0618 // Type or member is obsolete
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete


namespace PomCore;
/// <summary>
/// This is ManagedPlacedObjects mini-framework, written by henpemaz
/// and ported to Downpour by Thalber. 
/// It is licensed under Creative Commons 0 and is designed
/// to be "stackable" at runtime with multiple instances of POM
/// in multiple mods. It is not recommended to do that anymore, but
/// you still can if you really want to.
///
/// VectorList added by Cactus
/// </summary>
[BepInPlugin("rwmodding.coreorg.pom", "Pom", "3.0")]
public class Mod : BaseUnityPlugin
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

		try
		{
			IL.SaveUtils.PopulateUnrecognizedStringAttrs += __IL_FixUnrecognizedAttrs;
		}
		catch (System.Exception ex)
		{
			LogError($"Could not register Unrecognized fix ILHook : {ex}");
		}
	}
	/// <inheritdoc/>
	public void OnDisable()
	{
		Pom.Pom.DisposeStaticInputHooks();
	}

	internal void __IL_FixUnrecognizedAttrs(MonoMod.Cil.ILContext il)
	{
		//don't apply this hook in 1.9.14 or greater
		string version = RainWorld.GAME_VERSION_STRING;
		if (!version.StartsWith("v1.9.")) return; //some very different version
		version = version["v1.9.".Length..]; //remove all but the last number
		while (!version.IsNullOrWhiteSpace()) //don't try to parse whitespace
		{
			if (!int.TryParse(version, out var versionNumber)) //remove extra characters from the end, eg v1.9.15b
			{ version = version[..^1]; }
			else //if parsing is successful, we probably have a discernable version
			{
				if (versionNumber >= 14) return; //don't apply in 1.9.14 or greater
				else break; //continue on with the method
			}
		}

		MonoMod.Cil.ILCursor c = new(il);
		if (c.TryGotoNext(MonoMod.Cil.MoveType.After,
			(x) => x.MatchLdarg(1),
			(x) => x.MatchLdarg(0),
			(x) => x.MatchLdlen(),
			(x) => x.MatchConvI4(),
			(x) => x.MatchBlt(out ILLabel target)
			))
		{
			c.Prev.OpCode = OpCodes.Pop;
			c.Prev.Operand = null;
			c.Emit(OpCodes.Pop);
		}
		else
		{
			throw new Exception("Could not find injection point; presumed an identical hook is already in place");
		}
	}
}
