namespace Eff;

/// <summary>
/// Custom data description for a room effect.
/// </summary>
/// <param name="Category">Devtools category where the effect appears. Optional.</param>
/// <param name="Name">Name of the effect. Obligatory.</param>
/// <param name="UADFactory">Callback producing room objects with this effect on room load.</param>
/// <param name="EffectInitializer">Callback that runs on room load but does not produce an object.</param>
/// <param name="Fields">A dictionary view containing all custom fields of this effect.</param>
/// <returns></returns>
public sealed record EffectDefinition(
	DevInterface.RoomSettingsPage.DevEffectsCategories? Category,
	string Name,
	UADFactory? UADFactory,
	EffectInitializer? EffectInitializer,
	System.Collections.ObjectModel.ReadOnlyDictionary<string, EffectField> Fields
	)
{
	
	internal static EffectDefinition @default = new(null, "DefaultEffectDef", null, null, new(new Dictionary<string, EffectField>()));

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
