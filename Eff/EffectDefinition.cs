namespace Eff;

/// <summary>
/// Custom data description for a room effect.
/// </summary>
/// <param name="Category">Devtools category where the effect appears. Optional.</param>
/// <param name="Name">Name of the effect. Obligatory.</param>
/// <param name="UADFactory"></param>
/// <param name="Fields"></param>
/// <returns></returns>
public sealed record EffectDefinition(
	DevInterface.RoomSettingsPage.DevEffectsCategories? Category,
	string Name,
	Func<Room, EffectExtraData, FirstTimeRealized, UpdatableAndDeletable?>? UADFactory,
	System.Collections.ObjectModel.ReadOnlyDictionary<string, EffectField> Fields
	)
{
	
	internal static EffectDefinition @default = new(null, "DefaultEffectDef", null, new(new Dictionary<string, EffectField>()));

	private static void __ValidateDefaultValue(DataType t, object? value)
	{
		Type? needed = t switch
		{
			DataType.Int => typeof(int),
			DataType.Float => typeof(float),
			DataType.Bool => typeof(bool),
			DataType.String => typeof(string),
			_ => null
		};
		if (needed != value?.GetType())
		{
			throw new ArgumentException($"Invalid default argument value {value} for {t}");
		}
	}


}
