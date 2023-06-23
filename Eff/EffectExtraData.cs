namespace Pom.Eff;

public sealed class EffectExtraData
{
	public RoomSettings.RoomEffect Effect { get; private set; }
	public Dictionary<string, string> RawData { get; private set; }
	public EffectDefinition Definition { get; private set; }

	private Dictionary<string, Cached<int>> _ints = new();
	private Dictionary<string, Cached<float>> _floats = new();
	private Dictionary<string, Cached<bool>> _bools = new();
	private Dictionary<string, Cached<string>> _strings = new();
	public EffectExtraData(
		RoomSettings.RoomEffect effect,
		Dictionary<string, string> rawData,
		EffectDefinition definition)
	{
		Effect = effect;
		RawData = rawData;
		Definition = definition;
		foreach (var kvp in definition.fields)
		{
			string fieldname = kvp.Key;
			EffectField fielddef = kvp.Value;
			rawData.TryGetValue(fieldname, out string? fieldstringvalue);
			fieldstringvalue ??= "";
			// if (!definition.fields.TryGetValue(fieldname, out var fielddef))
			// {
			// 	fielddef = new StringField(fieldname, "");
			// }
			plog.LogDebug(fielddef);

			switch (fielddef)
			{
			case (IntField field):
			{
				if (!int.TryParse(fieldstringvalue, out var result)) result = field.DefaultInt;
				_ints[fieldname] = new(result, (newval) => rawData[fieldname] = newval.ToString());
				break;
			}
			case (FloatField field):
			{
				if (!float.TryParse(fieldstringvalue, out var result)) result = field.DefaultFloat;
				_floats[fieldname] = new(result, (newval) => rawData[fieldname] = newval.ToString());
				break;
			}
			case (BoolField field):
			{
				if (!bool.TryParse(fieldstringvalue, out var result)) result = field.DefaultBool;
				_bools[fieldname] = new(result, (newval) => rawData[fieldname] = newval.ToString());
				break;
			}
			case (StringField field):
			{
				//todo: escapes
				_strings[fieldname] = new(fieldstringvalue, (newval) => rawData[fieldname] = newval ?? field.DefaultString ?? "");
				break;
			}
			default:
			{
				plog.LogWarning($"Eff: Invalid default data setup for field {fieldname} : {fielddef}. Treating as string");
				_strings[fieldname] = new(fieldstringvalue, (newval) => rawData[fieldname] = newval);
				break;
			}
			}
		}
	}
}

#pragma warning restore 1591