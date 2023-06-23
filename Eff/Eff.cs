using Newtonsoft.Json;
namespace Pom.Eff;

/// <summary>
/// Functionality for extending effects with additional settings.
/// </summary>
public static partial class Eff
{
	public readonly static Dictionary<RoomSettings.RoomEffect, EffectExtraData> attachedData = new();
	public readonly static Dictionary<string, EffectDefinition> effectDefinitions = new();
	internal readonly static List<KeyValuePair<string, string>> __escapeSequences = new()
	{
		new("-", "%1"),
		new( ",","%2" ),
		new (":", "%3"),
		new("%","%0"), // this goes last, very important
	};
	internal static string __EscapeString(string s)
		=> __escapeSequences.Aggregate(s, (s, kvp) => s.Replace(kvp.Key, kvp.Value));
	internal static string __UnescapeString(string s)
		=> System.Linq.Enumerable.Reverse(__escapeSequences).Aggregate(s, (s, kvp) => s.Replace(kvp.Value, kvp.Key));


	private static FnOnce __Init = new(static () =>
	{
		plog.LogWarning("Eff init");
		On.RainWorldGame.ctor += (orig, self, manager) =>
		{
			orig(self, manager);
			attachedData.Clear();
		};
		On.RoomSettings.RoomEffect.FromString += (orig, self, data) =>
		{
			orig(self, data);
			plog.LogWarning($"Deserializing {self.type}");
			plog.LogWarning(effectDefinitions.TryGetValue(self.type.ToString(), out EffectDefinition? def));
			plog.LogWarning((self.unrecognizedAttributes?.Length ?? 0).ToString() ?? "EMPTY ATTRIBUTES");
			self.unrecognizedAttributes ??= new string[0];
			
			var newdata = new EffectExtraData(self, __ExtractRawExtraData(self), def ?? EffectDefinition.@default);
			attachedData[self] = newdata;
			plog.LogWarning(attachedData[self]);
		};
		On.RoomSettings.RoomEffect.ToString += (orig, self) =>
		{
			List<string> attributes = new();
			attributes.AddRange(self.unrecognizedAttributes ?? new string[0]);
			plog.LogWarning($"Serializing {self.type}");
			if (!attachedData.TryGetValue(self, out EffectExtraData data))
			{
				plog.LogWarning("Could not find EffectExtraData, aborting");
				goto done;
			}
			foreach (var kvp in data.Definition.Fields)
			{
				string fieldkey = kvp.Key;
				EffectField fielddef = kvp.Value;
				if (!data.RawData.TryGetValue(fieldkey, out string fieldval)) fieldval = fielddef.DefaultValue?.ToString() ?? "";
				plog.LogWarning($"serializing {fieldkey} : {fielddef} (value {fieldval})");
				attributes.Add($"{fieldkey}:{__EscapeString(fieldval)}");
			}
			self.unrecognizedAttributes = attributes.Count is 0 ? null : attributes.ToArray();
		//todo: test ser
		done:
			return orig(self);

		};

		//todo: add serialization
	});
	#region API
	public static void RegisterEffectDefinition(string name, EffectDefinition definition)
	{
		__Init.Invoke();
		if (__Init.error is Exception ex)
		{
			plog.LogFatal($"Error on Eff init {ex}");
		}
		if (!definition._sealed) throw new ArgumentException("Effect definition not sealed! Make sure to call Seal() after you are done adding fields");
		effectDefinitions[new RoomSettings.RoomEffect.Type(name, true).ToString()] = definition;
	}
	public static void RemoveEffectDefinition(string name)
	{
		effectDefinitions.Remove(name);
	}
	#endregion
	private static Dictionary<string, string> __ExtractRawExtraData(this RoomSettings.RoomEffect effect)
	{
		//List<int> popIndices = new();
		Dictionary<string, string> result = new();
		for (int i = 0; i < effect.unrecognizedAttributes.Length; i++)
		{
			ref string? attr = ref effect.unrecognizedAttributes[i];
			int splitindex = attr.IndexOf(':');
			if (splitindex < 0)
			{
				plog.LogError($"Eff: Unrecognized attribute needs a name! {attr} skipping");
				continue;
			}
			var name = attr.Substring(0, splitindex);
			var value = __UnescapeString(attr.Substring(splitindex + 1));
			plog.LogWarning($"Deserialized named property {name} : {value}");
			result[name] = value;
			attr = null; //remove from array
		}
		effect.unrecognizedAttributes = effect.unrecognizedAttributes.SkipWhile(x => x == null).ToArray();

		return result;
	}

}
