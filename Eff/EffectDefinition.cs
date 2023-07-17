namespace Eff;

public sealed record EffectDefinition(
	DevInterface.RoomSettingsPage.DevEffectsCategories? Category,
	string Name,
	Func<Room, EffectExtraData, FirstTimeRealized, UpdatableAndDeletable?>? UADFactory,
	System.Collections.ObjectModel.ReadOnlyDictionary<string, EffectField> Fields
	)
{
	public static EffectDefinition @default = new(null, "DefaultEffectDef", null, new(new Dictionary<string, EffectField>()));

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
