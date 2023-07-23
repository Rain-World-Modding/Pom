global using static PomCore.Mod;
global using static PomCore.Utils;
global using static PomCore.Logfix;
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
[BepInEx.BepInPlugin("rwmodding.coreorg.pom", "Pom", "2.7")]
public class Mod : BepInEx.BaseUnityPlugin
{
	/// <inheritdoc/>
	public void OnEnable()
	{
		try
		{
			Logfix.__SwitchToBepinexLogger(Logger);
			//instance = this;
			Eff.Examples.__RegisterExamples();
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

	internal static void __IL_FixUnrecognizedAttrs(MonoMod.Cil.ILContext il)
	{
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
	/// <inheritdoc/>
	public void OnDisable()
	{
		Pom.Pom.DisposeStaticInputHooks();
	}
}
