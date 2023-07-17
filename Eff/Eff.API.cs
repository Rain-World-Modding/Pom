namespace Eff;

public static partial class Eff
{
	/// <summary>
	/// Registers a new effect definition
	/// </summary>
	/// <param name="definition"></param>
	public static void RegisterEffectDefinition(EffectDefinition definition)
	{
		effectDefinitions[new RoomSettings.RoomEffect.Type(definition.Name, true).ToString()] = definition;
	}
	/// <summary>
	/// Removes an existing effect definition
	/// </summary>
	/// <param name="name"></param>
	public static void RemoveEffectDefinition(string name)
	{
		try
		{
			RoomSettings.RoomEffect.Type.values.RemoveEntry(name);
			effectDefinitions.Remove(name);
		}
		catch (Exception ex)
		{
			plog.LogError($"Could not unregister effect {name} : {ex}");
		}


	}
}