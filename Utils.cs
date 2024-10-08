using DevInterface;

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

	public static void SendSignal(
		this DevUINode devUINode,
		DevUISignalType signalType,
		DevUINode sender,
		string message)
	{
		while (devUINode != null)
		{
			devUINode = devUINode.parentNode;
			if (devUINode is IDevUISignals signals)
			{
				signals.Signal(signalType, sender, message);
				break;
			}
		}
	}
}
