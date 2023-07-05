using UnityEngine;
using DevInterface;

namespace Eff;

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
		On.Room.Loaded += (orig, self) =>
		{
			bool firstTimeRealized = self.abstractRoom.firstTimeRealized;
			orig(self);
			foreach (RoomSettings.RoomEffect effect in self.roomSettings.effects)
			{
				if (!effectDefinitions.TryGetValue(effect.type.ToString(), out EffectDefinition def))
				{
					continue;
				}
				if (!attachedData.TryGetValue(effect.GetHashCode(), out EffectExtraData data))
				{
					plog.LogDebug($"{effect.type} in {self.abstractRoom.name} has no attached data, can not run object factory");
					continue;
				}
				try
				{
					UpdatableAndDeletable? uad = def._UADFactory?.Invoke(self, data, firstTimeRealized ? FirstTimeRealized.Yes : FirstTimeRealized.No);
					if (uad is null) continue;
					plog.LogDebug($"Created an effect-UAD {uad} in room {self.abstractRoom.name}");
					self.AddObject(uad);
				}
				catch (Exception ex)
				{
					plog.LogWarning($"Error running effect-UAD factory for {def} in room {self.abstractRoom.name} : {ex}");
				}
			}
		};
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

		Vector2 shift = new(H_SPACING, V_SPACING);
		foreach ((var key, (var field, var cache)) in data._floats)
		{
			StretchBounds();
			(FloatField field, Cached<float> cache) value = (field, cache);
			plog.LogWarning($"Adding slider for {value}");
			var item = new CustomFloatSlider(owner, $"{key}_Slider", self, shift, $"{key}: ", value, effect);
			self.subNodes.Add(item);
		}
		foreach ((var key, (var field, var cache)) in data._ints)
		{
			StretchBounds();
			(IntField field, Cached<int> cache) value = (field, cache);
			plog.LogWarning($"Adding int buttons for {value}");
			Vector2 inRowShift = shift;
			DevUILabel labelName = new(owner, $"{key}_Fieldname", self, inRowShift, DEVUI_TITLE_WIDTH, field.Name);
			DevUILabel labelValue = new(owner, $"{key}_ValueLabel", self, inRowShift, INT_VALUELABEL_WIDTH, cache.val.ToString()); //buttons need it
			inRowShift.x += DEVUI_TITLE_WIDTH + H_SPACING;
			CustomIntButton buttonDec = new(owner, $"{key}_Decrease", self, inRowShift, CustomIntButton.BType.Decrement, value, labelValue);
			inRowShift.x += INT_BUTTON_WIDTH + H_SPACING;
			labelValue.pos = inRowShift;
			inRowShift.x += INT_VALUELABEL_WIDTH + H_SPACING;
			CustomIntButton buttonInc = new(owner, $"{key}_Increase", self, inRowShift, CustomIntButton.BType.Increment, value, labelValue);
			self.subNodes.AddRange(new DevUINode[] { labelName, buttonDec, labelValue, buttonInc });
		}
		foreach ((var key, (var field, var cache)) in data._bools)
		{
			StretchBounds();
			(BoolField field, Cached<bool> cache) value = (field, cache);
			plog.LogWarning($"Adding bool button for {value}");
			Vector2 inRowShift = shift;
			DevUILabel labelName = new(owner, $"{key}_Fieldname", self, inRowShift, DEVUI_TITLE_WIDTH, field.Name);
			inRowShift.x += DEVUI_TITLE_WIDTH + H_SPACING;
			CustomBoolButton buttonValue = new(owner, $"{key}_Toggle", self, inRowShift, value);
			self.subNodes.AddRange(new DevUINode[] { labelName, buttonValue });

		}


		void StretchBounds()
		{
			self.size.y += ROW_HEIGHT + V_SPACING;
			shift.y += ROW_HEIGHT + V_SPACING;
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