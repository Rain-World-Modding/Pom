namespace Eff;

/// <summary>
/// Functionality for extending effects with additional settings.
/// </summary>
public static partial class Eff
{
	static Eff()
	{
		try
		{
			__ImplInit();
		}
		catch (Exception ex)
		{
			plog.LogFatal(ex);
		}
	}

	private static void __ImplInit()
	{
		__AddHooks();
	}
	internal const float DEVUI_TITLE_WIDTH = 110f;
	internal const float V_SPACING = 5f;
	internal const float H_SPACING = 5f;
	internal const float ROW_HEIGHT = 18f;
	internal const float INT_BUTTON_WIDTH = 20f;
	internal const float INT_VALUELABEL_WIDTH = 60f;
	internal const float BOOL_BUTTON_WIDTH = 50f;
	internal readonly static Dictionary<int, EffectExtraData> attachedData = new();
	internal readonly static Dictionary<string, EffectDefinition> effectDefinitions = new();
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
			var name = __UnescapeString(attr.Substring(0, splitindex));
			var value = __UnescapeString(attr.Substring(splitindex + 1));
			plog.LogWarning($"Deserialized named property {name} : {value}");
			result[name] = value;
			attr = null; //remove from array
		}
		effect.unrecognizedAttributes = effect.unrecognizedAttributes.SkipWhile(x => x == null).ToArray();

		return result;
	}

}
