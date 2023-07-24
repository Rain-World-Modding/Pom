using DevInterface;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Eff;

public static partial class Eff
{
	internal static void __AddHooks()
	{
		const System.Reflection.BindingFlags ALLCTX = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
		LogWarning("Eff init");
		// On.RainWorldGame.ctor += __ClearAttachedData;
		On.RoomSettings.RoomEffect.FromString += __ParseExtraData;
		On.RoomSettings.RoomEffect.ToString += __SaveExtraData;
		On.ProcessManager.PostSwitchMainProcess += __ClearAttachedData;
		On.DevInterface.EffectPanel.ctor += __ConstructEffectPanel;
		On.Room.Loaded += __SpawnEffectObjects;
		new MonoMod.RuntimeDetour.Hook(
			typeof(Slider).GetMethod($"get_SliderStartCoord", ALLCTX | System.Reflection.BindingFlags.Instance),
			typeof(Eff).GetMethod(nameof(__OverrideSliderStartCoord), ALLCTX | System.Reflection.BindingFlags.Static));
		IL.DevInterface.RoomSettingsPage.AssembleEffectsPages += __IL_SortEffectEntries;
		On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += __EffectsPage_Sort;
	}
	private static float __OverrideSliderStartCoord(Func<Slider, float> orig, Slider self)
	{
		float res = orig(self);
		if (self is CustomFloatSlider cfs) res += CUSTOM_SLIDER_EXTRA_NUMBER_SPACE;
		return res;
	}

	private static void __SpawnEffectObjects(On.Room.orig_Loaded orig, Room self)
	{
		bool firstTimeRealized = self.abstractRoom.firstTimeRealized;
		orig(self);
		foreach (RoomSettings.RoomEffect effect in self.roomSettings.effects)
		{
			if (!__effectDefinitions.TryGetValue(effect.type.ToString(), out EffectDefinition def))
			{
				continue;
			}
			if (!__attachedData.TryGetValue(effect.GetHashCode(), out EffectExtraData data))
			{
				LogDebug($"{effect.type} in {self.abstractRoom.name} has no attached data, can not run object factory");
				continue;
			}
			try
			{
				def.EffectInitializer?.Invoke(self, data, firstTimeRealized);
			}
			catch (Exception ex)
			{
				LogWarning($"Error running effect-initializer for {def} in room {self.abstractRoom.name} : {ex}");
			}
			try
			{
				UpdatableAndDeletable? uad = def.UADFactory?.Invoke(self, data, firstTimeRealized);
				if (uad is null) continue;
				LogDebug($"Created an effect-UAD {uad} in room {self.abstractRoom.name}");
				self.AddObject(uad);
			}
			catch (Exception ex)
			{
				LogWarning($"Error running effect-UAD factory for {def} in room {self.abstractRoom.name} : {ex}");
			}
		}
	}

	private static void __ClearAttachedData(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID procID)
	{
		orig(self, procID);
		if (self.currentMainLoop is not RainWorldGame && !self.sideProcesses.Any((proc) => proc is RainWorldGame))
		{
			LogWarning("Clearing attached data");
			__attachedData.Clear();
		}
	}

	private static void __ConstructEffectPanel(On.DevInterface.EffectPanel.orig_ctor orig, EffectPanel self, DevUI owner, DevUINode parent, Vector2 pos, RoomSettings.RoomEffect effect)
	{
		orig(self, owner, parent, pos, effect);
		if (!__effectDefinitions.TryGetValue(effect.type.ToString(), out EffectDefinition def))
		{
			return;
		}
		if (!__attachedData.TryGetValue(effect.GetHashCode(), out EffectExtraData data))
		{
			//LogDebug($"{effect.type} ({effect.GetHashCode()}) has no additional data attached. {__attachedData.Count}, {def}");
			return;
		}

		Vector2 shift = new(H_SPACING, V_SPACING);
		foreach ((var key, (var field, var cache)) in data._floats)
		{
			StretchBounds();
			(EFloatField field, Cached<float> cache) value = (field, cache);
			LogDebug($"Adding slider for {value}");
			var item = new CustomFloatSlider(owner, $"{key}_Slider", self, shift, $"{field._ActualDisplayName} ", value, effect);
			self.subNodes.Add(item);
		}
		foreach ((var key, (var field, var cache)) in data._ints)
		{
			StretchBounds();
			(EIntField field, Cached<int> cache) value = (field, cache);
			LogDebug($"Adding int buttons for {value}");
			Vector2 inRowShift = shift;
			DevUILabel labelName = new(owner, $"{key}_Fieldname", self, inRowShift, DEVUI_TITLE_WIDTH, field._ActualDisplayName);
			DevUILabel labelValue = new(owner, $"{key}_ValueLabel", self, inRowShift, INT_VALUELABEL_WIDTH, cache.Value.ToString()); //buttons need it
			inRowShift.x += DEVUI_TITLE_WIDTH + H_SPACING;
			CustomIntButton buttonDec = new(
				owner,
				$"{key}_Decrease",
				self,
				inRowShift,
				CustomIntButton.BType.Decrement,
				value,
				labelValue,
				effect);
			inRowShift.x += INT_BUTTON_WIDTH + H_SPACING;
			labelValue.pos = inRowShift;
			inRowShift.x += INT_VALUELABEL_WIDTH + H_SPACING;
			CustomIntButton buttonInc = new(
				owner,
				$"{key}_Increase",
				self,
				inRowShift,
				CustomIntButton.BType.Increment,
				value,
				labelValue,
				effect);
			self.subNodes.AddRange(new DevUINode[] { labelName, buttonDec, labelValue, buttonInc });
		}
		foreach ((var key, (var field, var cache)) in data._bools)
		{
			StretchBounds();
			(EBoolField field, Cached<bool> cache) value = (field, cache);
			LogDebug($"Adding bool button for {value}");
			Vector2 inRowShift = shift;
			DevUILabel labelName = new(owner, $"{key}_Fieldname", self, inRowShift, DEVUI_TITLE_WIDTH, field._ActualDisplayName);
			inRowShift.x += DEVUI_TITLE_WIDTH + H_SPACING;
			CustomBoolButton buttonValue = new(
				owner,
				$"{key}_Toggle",
				self,
				inRowShift,
				value,
				effect);
			self.subNodes.AddRange(new DevUINode[] { labelName, buttonValue });
		}
		foreach ((var key, (var field, var cache)) in data._strings)
		{
			StretchBounds();
			(EStringField field, Cached<string> cache) value = (field, cache);
			LogDebug($"Adding string panel for {value}");
			Vector2 inRowShift = shift;
			DevUILabel labelName = new(owner, $"{key}_Fieldname", self, inRowShift, DEVUI_TITLE_WIDTH, field._ActualDisplayName);
			inRowShift.x += DEVUI_TITLE_WIDTH + H_SPACING;
			CustomStringPanel panelValue = new(
				owner,
				$"{key}_ValuePanel",
				self,
				inRowShift,
				self.size.x - (inRowShift.x + H_SPACING),
				value,
				effect);

			// CustomBoolButton buttonValue = new(
			// 	owner,
			// 	$"{key}_Toggle",
			// 	self,
			// 	inRowShift,
			// 	value,
			// 	effect);
			self.subNodes.AddRange(new DevUINode[] { labelName, panelValue });
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
		LogWarning("Clearing attached data");
		__attachedData.Clear();
	}

	private static void __ParseExtraData(On.RoomSettings.RoomEffect.orig_FromString orig, RoomSettings.RoomEffect self, string[] text)
	{
		orig(self, text);
		__effectDefinitions.TryGetValue(self.type.ToString(), out EffectDefinition? def);
		LogWarning($"Deserializing {self.type}, {self.GetHashCode()}, {def}");
		LogWarning((self.unrecognizedAttributes?.Length ?? 0).ToString() ?? "EMPTY ATTRIBUTES");
		self.unrecognizedAttributes ??= new string[0];

		EffectExtraData newdata = new EffectExtraData(self, __ExtractRawExtraData(self), def ?? EffectDefinition.@default);
		__attachedData[self.GetHashCode()] = newdata;
		LogWarning(__attachedData[self.GetHashCode()]);
	}

	private static string __SaveExtraData(On.RoomSettings.RoomEffect.orig_ToString orig, RoomSettings.RoomEffect self)
	{
		List<string> attributes = new();
		attributes.AddRange(self.unrecognizedAttributes ?? new string[0]);
		// plog.LogWarning($"Serializing {self.type}");
		if (!__attachedData.TryGetValue(self.GetHashCode(), out EffectExtraData data))
		{
			LogWarning("Could not find EffectExtraData, aborting");
			goto done;
		}
		foreach (var kvp in data.RawData)
		{
			string fieldkey = kvp.Key;
			string fieldval = kvp.Value ?? "";
			//not discarding unknown data
			//if (!data.RawData.TryGetValue(fieldkey, out string fieldval)) fieldval = fielddef.ToString() ?? "";
			LogWarning($"serializing {fieldkey} : {fieldval} (value {fieldval})");
			attributes.Add($"{__EscapeString(fieldkey)}:{__EscapeString(fieldval)}");
		}
		self.unrecognizedAttributes = attributes.Count is 0 ? null : attributes.ToArray();
	done:
		return orig(self);
	}
	private static DevInterface.RoomSettingsPage.DevEffectsCategories __EffectsPage_Sort(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, DevInterface.RoomSettingsPage self, RoomSettings.RoomEffect.Type type)
	{
		if (__effectCategories.TryGetValue(type.value, out var cat))
		{
			LogDebug($"Sorting {type} into {cat}");
			return cat;
		}
		return orig(self, type);
	}
	private static void __IL_SortEffectEntries(MonoMod.Cil.ILContext il)
	{
		LogDebug("Effects ILHook body start");
		MonoMod.Cil.ILCursor c = new(il);
		c.GotoNext(MonoMod.Cil.MoveType.Before,
			//x => x.MatchLdcI4(0),
			x => x.MatchStloc(2),
			x => x.MatchLdarg(0),
			x => x.MatchLdloc(1),
			x => x.MatchNewarr<RoomSettings.RoomEffect.Type>()
			);//TryFindNext(out )
		LogDebug($"Found inj point, emitting");
		//c.Remove();
		c.Emit(OpCodes.Pop);
		c.Emit(OpCodes.Ldloc_0);
		//c.Index -= 1;
		//var newLabel = c.MarkLabel();
		//c.Index += 1;
		c.EmitDelegate((Dictionary<DevInterface.RoomSettingsPage.DevEffectsCategories, List<PlacedObject.Type>> dict) =>
		{
			LogDebug("Sorter ilhook go");
			foreach (var kvp in dict)
			{
				var cat = kvp.Key;
				var list = kvp.Value;
				if (!__sortCategorySettings.TryGetValue(cat, out CategorySortKind doSort)) doSort = __sortByDefault;
				if (doSort is CategorySortKind.Default)
				{
					LogDebug($"Sorting of {cat} not required");
					continue;
				}
				System.Comparison<PlacedObject.Type> sorter = doSort switch
				{
					CategorySortKind.Alphabetical => static (ot1, ot2) =>
					{
						if (ot1 == PlacedObject.Type.None && ot2 == PlacedObject.Type.None) return 0;
						if (ot1 == PlacedObject.Type.None) return 1;
						if (ot2 == PlacedObject.Type.None) return -1;
						else return StringComparer.InvariantCulture.Compare(ot1?.value, ot2?.value);
					}
					,
					_ => throw new ArgumentException($"ERROR: INVALID {nameof(CategorySortKind)} VALUE {doSort}")
				};
				list.Sort(sorter);
				LogDebug($"sorting of {cat} completed ({list.Count} items)");
			}
		});
		c.Emit(OpCodes.Ldc_I4_0);
		LogDebug("emit complete");
		//plog.LogDebug(il.ToString());
	}
	/// <summary>
	/// How to sort object entries inside a devtools object category
	/// </summary>
	public enum CategorySortKind
	{
		/// <summary>
		/// No sorting, items in the order they were registered
		/// </summary>
		Default,
		/// <summary>
		/// Alphabetical sorting (invariant culture)
		/// </summary>
		Alphabetical,
	}
}