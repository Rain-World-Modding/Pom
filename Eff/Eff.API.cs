namespace Eff;

public static partial class Eff
{
	/// <summary>
	/// Registers a new effect definition
	/// </summary>
	/// <param name="definition"></param>
	public static void RegisterEffectDefinition(EffectDefinition definition)
	{
		//if (effectDefinitions.ContainsKey())
		var type = new RoomSettings.RoomEffect.Type(definition.Name, true);
		if (!effectDefinitions.TryAdd(type.ToString(), definition))
		{
			throw new System.ArgumentException(
				$"There is already an effect definition for {type.ToString()}; cannot register twice. " +
				"If you wish to replace your definition for whatever reason, call RemoveEffectDefinition.");
		}
	}
	/// <summary>
	/// Removes an existing effect definition
	/// </summary>
	/// <param name="name"></param>
	public static void RemoveEffectDefinition(string name)
	{
		try
		{

			if (effectDefinitions.Remove(name))
			{
				RoomSettings.RoomEffect.Type.values.RemoveEntry(name);
			}
			else
			{
				LogWarning($"Definition for {name} has not been registered; nothing to unregister.");
			}
		}
		catch (Exception ex)
		{
			LogError($"Could not unregister effect {name} : {ex}");
		}


	}
}