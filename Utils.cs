namespace PomCore;

internal static class Utils
{
	internal static bool TryAdd<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV value)
	{
		if (dict.ContainsKey(key)) return false;
		dict.Add(key, value);
		return true;
	}
	internal static void Deconstruct<TK, TV>(this KeyValuePair<TK, TV> kvp, out TK k, out TV v)
	{
		k = kvp.Key;
		v = kvp.Value;
	}
	
}