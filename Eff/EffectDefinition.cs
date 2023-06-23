namespace Pom.Eff;

public sealed class EffectDefinition
{
    public DevInterface.RoomSettingsPage.DevEffectsCategories? category;
	public Dictionary<string, EffectField> fields { get; set; } = new();
	public static EffectDefinition @default = new();

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

    public EffectDefinition AddField(EffectField field) {
        fields[field.Name] = field;
        return this;
    }
	//todo: impl builder pattern for defining fields
}

#pragma warning restore 1591