namespace Eff;

/// <summary>
/// Carries additional data that is attached to a given instance of <see cref="global::RoomSettings.RoomEffect"/>.
/// To get values of your effect's settings, see contents of <see cref="Ints"/>, <see cref="Floats"/>, <see cref="Bools"/>, and <see cref="Strings"/> properties. Values are accessed through <see cref="Cached{T}"/>.
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

	internal Dictionary<string, (IntField fieldDef, Cached<int> valueCache)> _ints = new();
	/// <summary>
	/// All integer fields for this effect.
	/// </summary>
	public System.Collections.ObjectModel.ReadOnlyDictionary<string, (IntField fieldDef, Cached<int> valueCache)> Ints { get; }
	internal Dictionary<string, (FloatField fieldDef, Cached<float> valueCache)> _floats = new();
	/// <summary>
	/// All float fields for this effect.
	/// </summary>
	public System.Collections.ObjectModel.ReadOnlyDictionary<string, (FloatField fieldDef, Cached<float> valueCache)> Floats { get; }
	internal Dictionary<string, (BoolField fieldDef, Cached<bool> valueCache)> _bools = new();
	/// <summary>
	/// All boolean fields for this effect.
	/// </summary>
	public System.Collections.ObjectModel.ReadOnlyDictionary<string, (BoolField fieldDef, Cached<bool> valueCache)> Bools { get; }
	internal Dictionary<string, (StringField fieldDef, Cached<string> valueCache)> _strings = new();
	/// <summary>
	/// All string fields for this effect.
	/// </summary>
	public System.Collections.ObjectModel.ReadOnlyDictionary<string, (StringField fieldDef, Cached<string> valueCache)> Strings { get; }
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
		Ints = new(_ints);
		Floats = new(_floats);
		Bools = new(_bools);
		Strings = new(_strings);
		Effect = effect;
		RawData = rawData;
		Definition = definition;
		foreach (var kvp in definition.Fields)
		{
			string fieldname = kvp.Key;
			EffectField fielddef = kvp.Value;
			if (!rawData.TryGetValue(fieldname, out string? fieldstringvalue))
			{
				plog.LogWarning($"Missing data entry for {fieldname}. Possible version mismatch");
				fieldstringvalue = fielddef.DefaultValue?.ToString();
			};
			fieldstringvalue ??= "";
			plog.LogDebug(fielddef);
			plog.LogDebug(fieldstringvalue);

			switch (fielddef)
			{
			case (IntField field):
			{
				if (!int.TryParse(fieldstringvalue, out var result)) result = field.DefaultInt;
				_ints[fieldname] = (field, new(result, (newval) => rawData[fieldname] = newval.ToString()));
				break;
			}
			case (FloatField field):
			{
				if (!float.TryParse(fieldstringvalue, out var result)) result = field.DefaultFloat;
				_floats[fieldname] = (field, new(result, (newval) => rawData[fieldname] = newval.ToString()));
				break;
			}
			case (BoolField field):
			{
				if (!bool.TryParse(fieldstringvalue, out var result)) result = field.DefaultBool;
				_bools[fieldname] = (field, new(result, (newval) => rawData[fieldname] = newval.ToString()));
				break;
			}
			case (StringField field):
			{
				//todo: escapes
				_strings[fieldname] = (field, new(fieldstringvalue, (newval) => rawData[fieldname] = newval ?? field.DefaultString ?? ""));
				break;
			}
			default:
			{
				plog.LogWarning($"Eff: Invalid default data setup for field {fieldname} : {fielddef} : {fielddef.DefaultValue}. Discarding");
				//_strings[fieldname] = (field, new(fieldstringvalue, (newval) => rawData[fieldname] = newval));
				break;
			}
			}
		}
	}
}

#pragma warning restore 1591