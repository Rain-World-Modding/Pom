﻿global using static PomCore.Mod;
global using static PomCore.Utils;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;

// This is ManagedPlacedObjects mini-framework, written by henpemaz
// and ported to Downpour by Thalber. 
// It is licensed under Creative Commons 0 and is designed
// to be "stackable" at runtime with multiple instances of POM
// in multiple mods. It is not recommended to do that anymore, but
// you still can if you really want to.


namespace PomCore;

[BepInEx.BepInPlugin("rwmodding.coreorg.pom", "Pom", "2.7")]
public class Mod : BepInEx.BaseUnityPlugin
{
	internal static Mod instance = null!;
	internal static BepInEx.Logging.ManualLogSource plog => instance.Logger; //new BepInEx.Logging.ManualLogSource("POM");//instance.Logger;
	public void OnEnable()
	{
		try
		{
			instance = this;
			new Eff.EffectDefinitionBuilder("testeffect")

					.AddField(new Eff.IntField("intfield", 0, 10, 5))
					.AddField(new Eff.FloatField("floatfield", 0f, 10f, 0.1f, 1f))
					// .AddField(new Eff.FloatField("testfield22", 0f, 10f, 0.1f, 1f))
					// .AddField(new Eff.FloatField("testfield23", 0f, 10f, 5f, 1f))
					.AddField(new Eff.BoolField("boolfield", true))
					.AddField(new Eff.StringField("stringfield", "example_string%-%"))
					.SetUADFactory((room, data, firstTimeRealized) => new Eff.ExampleEffectUAD(data))
					.Register();
			// Eff.Eff.RegisterEffectDefinition
			// (

			// );
		}
		catch (Exception ex)
		{
			plog.LogFatal(ex);
		}
		try
		{
			IL.SaveUtils.PopulateUnrecognizedStringAttrs += IL_FixUnrecognizedAttrs;
		}
		catch (System.Exception ex)
		{
			plog.LogError($"Could not register Unrecognized fix ILHook : {ex}");
		}
	}

	public static void IL_FixUnrecognizedAttrs(MonoMod.Cil.ILContext il)
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

			//plog.LogDebug(il.ToString());
		}
		else
		{
			throw new Exception("Could not find injection point; presumed an identical hook is already in place");
		}
	}

	public void OnDisable()
	{
		Pom.Pom.DisposeStaticInputHooks();
	}
}
