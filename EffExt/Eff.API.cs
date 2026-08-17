namespace EffExt;

public static partial class Eff
{
	/// <summary>
	/// Registers a new effect definition
	/// </summary>
	/// <param name="definition"></param>
	public static void RegisterEffectDefinition(EffectDefinition definition)
	{
		//if (effectDefinitions.ContainsKey())
		var type = new RoomSettings.RoomEffect.Type(definition.Name, true);
		if (!__effectDefinitions.TryAdd(type.ToString(), definition))
		{
			throw new System.ArgumentException(
				$"There is already an effect definition for {type.ToString()}; cannot register twice. " +
				"If you wish to replace your definition for whatever reason, call RemoveEffectDefinition first.");
		}
		if (definition.Category is DevInterface.RoomSettingsPage.DevEffectsCategories cat)
		{
			__effectCategories[definition.Name] = cat;
		}
	}
	/// <summary>
	/// Removes an existing effect definition
	/// </summary>
	/// <param name="name"></param>
	public static void RemoveEffectDefinition(string name)
	{
		try
		{
			if (__effectDefinitions.Remove(name))
			{
				RoomSettings.RoomEffect.Type.values.RemoveEntry(name);
				RoomSettings.RoomEffect.Type.valuesVersion++;
			}
			else
			{
				LogWarning($"Definition for {name} has not been registered; nothing to unregister.");
			}
		}
		catch (Exception ex)
		{
			LogError($"Could not unregister effect {name} : {ex}");
		}
	}
	/// <summary>
	/// Attempts to fetch definition for a given effect type.
	/// </summary>
	/// <param name="effectType">Effect type.</param>
	/// <param name="result">Contains operation result by the time method exits. Null if not found.</param>
	/// <returns>Whether an effect definition was found.</returns>
	public static bool TryGetEffectDefinition(RoomSettings.RoomEffect.Type effectType, out EffectDefinition? result)
		=> TryGetEffectDefinition(effectType.value, out result);

	/// <summary>
	/// Attempts to fetch definition for effect type with a given name.
	/// </summary>
	/// <param name="effectType">Effect type in form of a string (ExtEnum.value).</param>
	/// <param name="result">Contains operation result by the time method exits. Null if not found.</param>
	/// <returns>Whether an effect definition was found.</returns>
	private static bool TryGetEffectDefinition(string effectType, out EffectDefinition? result)
		=> __effectDefinitions.TryGetValue(effectType, out result);

	/// <summary>
	/// Attempts to fetch extra data for an effect type in a room settings
	/// </summary>
	/// <param name="settings">The room settings to fetch from</param>
	/// <param name="effectType">The effect type to grab</param>
	/// <param name="result">Extra data, if it exists and is present</param>
	/// <returns>Whether effect data was found</returns>
	public static bool TryGetExtraData(this RoomSettings settings, RoomSettings.RoomEffect.Type effectType, out EffectExtraData? result)
	{
		result = null; // base case
		return settings.GetEffect(effectType) is { } effect && TryGetExtraData(effect, out result);
	}

	/// <summary>
	/// Atempts to fetch extra data for an effect instance
	/// </summary>
	/// <param name="effect">Effect instance</param>
	/// <param name="result">Extra data, if present</param>
	/// <returns>Whether effect data was found</returns>
	public static bool TryGetExtraData(RoomSettings.RoomEffect effect, out EffectExtraData? result)
		=> __attachedData.TryGetValue(effect, out result);
}
