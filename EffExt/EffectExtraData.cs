namespace EffExt;

/// <summary>
/// Carries additional data that is attached to a given instance of <see cref="global::RoomSettings.RoomEffect"/>.
/// </summary>
public sealed class EffectExtraData
{
	/// <summary>
	/// Effect instance this ExtraData is attached to.
	/// </summary>
	public RoomSettings.RoomEffect Effect { get; private set; }
	/// <summary>
	/// Dictionary containing raw string values of all fields. May contain data that has not been bound to a specific effect field (such as when a version change removed a field)
	/// </summary>
	public Dictionary<string, string> RawData { get; private set; }
	/// <summary>
	/// The definition of this effect. Describes what fields it should contain.
	/// </summary>
	public EffectDefinition Definition { get; private set; }

	internal Dictionary<string, (EIntField fieldDef, Cached<int> valueCache)> _ints = new();
	// /// <summary>
	// /// All integer fields for this effect.
	// /// </summary>
	// public System.Collections.ObjectModel.ReadOnlyDictionary<string, (IntField fieldDef, Cached<int> valueCache)> Ints { get; }
	internal Dictionary<string, (EFloatField fieldDef, Cached<float> valueCache)> _floats = new();
	// /// <summary>
	// /// All float fields for this effect.
	// /// </summary>
	// public System.Collections.ObjectModel.ReadOnlyDictionary<string, (FloatField fieldDef, Cached<float> valueCache)> Floats { get; }
	internal Dictionary<string, (EBoolField fieldDef, Cached<bool> valueCache)> _bools = new();
	// /// <summary>
	// /// All boolean fields for this effect.
	// /// </summary>
	// public System.Collections.ObjectModel.ReadOnlyDictionary<string, (BoolField fieldDef, Cached<bool> valueCache)> Bools { get; }
	internal Dictionary<string, (EStringField fieldDef, Cached<string> valueCache)> _strings = new();
	// /// <summary>
	// /// All string fields for this effect.
	// /// </summary>
	// public System.Collections.ObjectModel.ReadOnlyDictionary<string, (StringField fieldDef, Cached<string> valueCache)> Strings { get; }
	/// <summary>
	/// Mirrors <see cref="global::RoomSettings.RoomEffect.amount"/>
	/// </summary>
	public float Amount => Effect.amount;
	/// <summary>
	/// Mirrors <see cref="global::RoomSettings.RoomEffect.type"/>
	/// </summary>
	public RoomSettings.RoomEffect.Type EffectType => Effect.type;

	internal EffectExtraData(
		RoomSettings.RoomEffect effect,
		Dictionary<string, string> rawData,
		EffectDefinition definition)
	{
		// Ints = new(_ints);
		// Floats = new(_floats);
		// Bools = new(_bools);
		// Strings = new(_strings);
		Effect = effect;
		RawData = rawData;
		Definition = definition;
		foreach (var kvp in definition.Fields)
		{
			string fieldname = kvp.Key;
			EffectField fielddef = kvp.Value;
			if (!rawData.TryGetValue(fieldname, out string? fieldstringvalue))
			{
				LogWarning($"Missing data entry for {fieldname}. Possible version mismatch");
				fieldstringvalue = fielddef.DefaultValue?.ToString();
			};
			fieldstringvalue ??= "";
			LogDebug(fielddef);
			LogDebug(fieldstringvalue);

			switch (fielddef)
			{
			case (EIntField field):
			{
				if (!int.TryParse(fieldstringvalue, out var result)) result = field.DefaultInt;
				_ints[fieldname] = (field, new(result, (newval) => rawData[fieldname] = newval.ToString()));
				break;
			}
			case (EFloatField field):
			{
				if (!float.TryParse(fieldstringvalue, out var result)) result = field.DefaultFloat;
				_floats[fieldname] = (field, new(result, (newval) => rawData[fieldname] = newval.ToString()));
				break;
			}
			case (EBoolField field):
			{
				if (!bool.TryParse(fieldstringvalue, out var result)) result = field.DefaultBool;
				_bools[fieldname] = (field, new(result, (newval) => rawData[fieldname] = newval.ToString()));
				break;
			}
			case (EStringField field):
			{
				//todo: escapes
				_strings[fieldname] = (field, new(fieldstringvalue, (newval) => rawData[fieldname] = newval ?? field.DefaultString ?? ""));
				break;
			}
			default:
			{
				LogWarning($"Eff: Invalid default data setup for field {fieldname} : {fielddef} : {fielddef.DefaultValue}. Discarding");
				//_strings[fieldname] = (field, new(fieldstringvalue, (newval) => rawData[fieldname] = newval));
				break;
			}
			}
		}
	}

	/// <summary>
	/// Gets an integer value with a given name. Throws if key is not found.
	/// </summary>
	/// <param name="key">Name of the field</param>
	/// <returns>Resulting value.</returns>
	public int GetInt(string key)
	{
		if (!_ints.TryGetValue(key, out (EIntField fieldDef, Cached<int> valueCache) data))
		{
			throw new KeyNotFoundException($"{EffectType} has not been given a field called {key}");
		}
		return data.valueCache.Value;
	}
	/// <summary>
	/// Gets a float value with a given name. Throws if key is not found.
	/// </summary>
	/// <param name="key">Name of the field</param>
	/// <returns>Resulting value.</returns>
	public float GetFloat(string key)
	{
		if (!_floats.TryGetValue(key, out (EFloatField fieldDef, Cached<float> valueCache) data))
		{
			throw new KeyNotFoundException($"{EffectType} has not been given a field called {key}");
		}
		return data.valueCache.Value;
	}
	/// <summary>
	/// Gets a boolean value with a given name. Throws if key is not found.
	/// </summary>
	/// <param name="key">Name of the field</param>
	/// <returns>Resulting value.</returns>
	public bool GetBool(string key)
	{
		if (!_bools.TryGetValue(key, out var data))
		{
			throw new KeyNotFoundException($"{EffectType} has not been given a field called {key}");
		}
		return data.valueCache.Value;
	}
	/// <summary>
	/// Gets a string value with a given name. Throws if key is not found.
	/// </summary>
	/// <param name="key">Name of the field</param>
	/// <returns>Resulting value.</returns>
	public string GetString(string key)
	{
		if (!_strings.TryGetValue(key, out var data))
		{
			throw new KeyNotFoundException($"{EffectType} has not been given a field called {key}");
		}
		return data.valueCache.Value;
	}
	/// <summary>
	/// Sets an int with a given key. Throws if key is not found.
	/// </summary>
	/// <param name="key">Name of the field</param>
	/// <param name="value">Value to be set</param>
	public void Set(string key, int value)
	{
		if (!_ints.TryGetValue(key, out (EIntField fieldDef, Cached<int> valueCache) data))
		{
			throw new KeyNotFoundException($"{EffectType} has not been given a field called {key}");
		}
		data.valueCache.Value = value;
	}
	/// <summary>
	/// Sets a float with a given key. Throws if key is not found.
	/// </summary>
	/// <param name="key">Name of the field</param>
	/// <param name="value">Value to be set</param>
	public void Set(string key, float value)
	{
		if (!_floats.TryGetValue(key, out (EFloatField fieldDef, Cached<float> valueCache) data))
		{
			throw new KeyNotFoundException($"{EffectType} has not been given a field called {key}");
		}
		data.valueCache.Value = value;
	}
	/// <summary>
	/// Sets a bool with a given key. Throws if key is not found.
	/// </summary>
	/// <param name="key">Name of the field</param>
	/// <param name="value">Value to be set</param>
	public void Set(string key, bool value)
	{
		if (!_bools.TryGetValue(key, out var data))
		{
			throw new KeyNotFoundException($"{EffectType} has not been given a field called {key}");
		}
		data.valueCache.Value = value;
	}
	/// <summary>
	/// Sets a string with a given key. Throws if key is not found.
	/// </summary>
	/// <param name="key">Name of the field</param>
	/// <param name="value">Value to be set</param>
	public void Set(string key, string value)
	{
		if (!_strings.TryGetValue(key, out var data))
		{
			throw new KeyNotFoundException($"{EffectType} has not been given a field called {key}");
		}
		data.valueCache.Value = value;
	}
}

#pragma warning restore 1591