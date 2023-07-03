using UnityEngine;
using DevInterface;

namespace Pom.Eff;

public static partial class Eff
{
	public static void __AddHooks()
	{
		plog.LogWarning("Eff init");
		// On.RainWorldGame.ctor += __ClearAttachedData;

		On.RoomSettings.RoomEffect.FromString += __ParseExtraData;
		On.RoomSettings.RoomEffect.ToString += __SaveExtraData;
		On.ProcessManager.PostSwitchMainProcess += (orig, self, procID) =>
		{
			orig(self, procID);
			if (self.currentMainLoop is not RainWorldGame && !self.sideProcesses.Any((proc) => proc is RainWorldGame))
			{
				plog.LogWarning("Clearing attached data");
				attachedData.Clear();
			}
		};
		On.DevInterface.EffectPanel.ctor += __ConstructEffectPanel;
	}

	private static void __ConstructEffectPanel(On.DevInterface.EffectPanel.orig_ctor orig, EffectPanel self, DevUI owner, DevUINode parent, Vector2 pos, RoomSettings.RoomEffect effect)
	{
		orig(self, owner, parent, pos, effect);
		effectDefinitions.TryGetValue(effect.type.ToString(), out EffectDefinition? def);
		if (!attachedData.TryGetValue(effect.GetHashCode(), out EffectExtraData data))
		{
			plog.LogDebug($"{effect.type} ({effect.GetHashCode()}) has no additional data attached. {attachedData.Count}, {def}");
			return;
		}
		Vector2 shift = new(5f, 20f);
		foreach ((var key, (var field, var cache)) in data._floats)
		{
			if (key is null || field is null || cache is null)
			{
				plog.LogError($"{key} {field} {cache}");
				continue;
			}

			(FloatField field, Cached<float> cache) value = (field, cache);
			plog.LogWarning($"Adding slider for {value}");
			self.subNodes.Add(new CustomFloatSlider(owner, $"{key}_Slider", self, shift, $"{key}: ", value, effect));
			shift.y += 20f;
			self.size += new Vector2(0f, 20f);
		}
	}

	private static void __ClearAttachedData(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
	{
		orig(self, manager);
		plog.LogWarning("Clearing attached data");
		attachedData.Clear();
	}

	private static void __ParseExtraData(On.RoomSettings.RoomEffect.orig_FromString orig, RoomSettings.RoomEffect self, string[] text)
	{
		orig(self, text);
		effectDefinitions.TryGetValue(self.type.ToString(), out EffectDefinition? def);
		plog.LogWarning($"Deserializing {self.type}, {self.GetHashCode()}, {def}");
		plog.LogWarning((self.unrecognizedAttributes?.Length ?? 0).ToString() ?? "EMPTY ATTRIBUTES");
		self.unrecognizedAttributes ??= new string[0];

		EffectExtraData newdata = new EffectExtraData(self, __ExtractRawExtraData(self), def ?? EffectDefinition.@default);
		attachedData[self.GetHashCode()] = newdata;
		plog.LogWarning(attachedData[self.GetHashCode()]);
	}

	private static string __SaveExtraData(On.RoomSettings.RoomEffect.orig_ToString orig, RoomSettings.RoomEffect self)
	{
		List<string> attributes = new();
		attributes.AddRange(self.unrecognizedAttributes ?? new string[0]);
		// plog.LogWarning($"Serializing {self.type}");
		if (!attachedData.TryGetValue(self.GetHashCode(), out EffectExtraData data))
		{
			plog.LogWarning("Could not find EffectExtraData, aborting");
			goto done;
		}
		foreach (var kvp in data.RawData)
		{
			string fieldkey = kvp.Key;
			string fieldval = kvp.Value ?? "";
			//not discarding unknown data
			//if (!data.RawData.TryGetValue(fieldkey, out string fieldval)) fieldval = fielddef.ToString() ?? "";
			plog.LogWarning($"serializing {fieldkey} : {fieldval} (value {fieldval})");
			attributes.Add($"{fieldkey}:{__EscapeString(fieldval)}");
		}
		self.unrecognizedAttributes = attributes.Count is 0 ? null : attributes.ToArray();
	//todo: test ser
	done:
		return orig(self);
	}
}