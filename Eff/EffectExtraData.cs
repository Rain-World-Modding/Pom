namespace Eff;

public sealed class EffectExtraData
{
	public RoomSettings.RoomEffect Effect { get; private set; }
	public Dictionary<string, string> RawData { get; private set; }
	public EffectDefinition Definition { get; private set; }

	internal Dictionary<string, (IntField fieldDef, Cached<int> valueCache)> _ints = new();
	internal System.Collections.ObjectModel.ReadOnlyDictionary<string, (IntField fieldDef, Cached<int> valueCache)> Ints { get; }
	internal Dictionary<string, (FloatField fieldDef, Cached<float> valueCache)> _floats = new();
	internal System.Collections.ObjectModel.ReadOnlyDictionary<string, (FloatField fieldDef, Cached<float> valueCache)> Floats { get; }
	internal Dictionary<string, (BoolField fieldDef, Cached<bool> valueCache)> _bools = new();
	internal System.Collections.ObjectModel.ReadOnlyDictionary<string, (BoolField fieldDef, Cached<bool> valueCache)> Bools { get; }
	internal Dictionary<string, (StringField fieldDef, Cached<string> valueCache)> _strings = new();
	internal System.Collections.ObjectModel.ReadOnlyDictionary<string, (StringField fieldDef, Cached<string> valueCache)> Strings { get; }

	public float Amount => Effect.amount;
	public RoomSettings.RoomEffect.Type EffectType => Effect.type;

	public EffectExtraData(
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
		foreach (var kvp in definition._fields)
		{
			string fieldname = kvp.Key;
			EffectField fielddef = kvp.Value;
			rawData.TryGetValue(fieldname, out string? fieldstringvalue);
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