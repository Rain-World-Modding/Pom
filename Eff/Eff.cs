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
			effectDefinitions.TryGetValue(self.type.ToString(), out EffectDefinition? def);
			var newdata = new EffectExtraData(self, __ExtractRawExtraData(self), def ?? EffectDefinition.@default);
			attachedData[self] = newdata;
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
		effectDefinitions[new RoomSettings.RoomEffect.Type(name, true).ToString()] = definition;
	}
	#endregion
	private static Dictionary<string, string> __ExtractRawExtraData(this RoomSettings.RoomEffect effect)
	{
		//List<int> popIndices = new();
		Dictionary<string, string> result = new();
		effect.unrecognizedAttributes ??= new string[0];
		for (int i = 0; i < effect.unrecognizedAttributes.Length; i++)
		{
			ref string? attr = ref effect.unrecognizedAttributes[i];
			if (attr is null) continue;
			int splitindex = attr.IndexOf(':');
			if (splitindex < 0)
			{
				plog.LogError($"Eff: Unrecognized attribute needs a name! {attr} skipping");
				continue;
			}
			var name = attr.Substring(0, splitindex);
			var value = attr.Substring(splitindex);
			result[name] = value;
			attr = null; //remove from array
		}
		effect.unrecognizedAttributes = effect.unrecognizedAttributes.SkipWhile(x => x == null).ToArray();
		return result;
	}

}
