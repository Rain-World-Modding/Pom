namespace Eff;

/// <summary>
/// Base class for effect fields.
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="Dt">Type of data</param>
/// <param name="DefaultValue">Default value</param>
/// <param name="DisplayName">How the field's name should appear to end user. Defaults to Name.</param>
public abstract record EffectField(string Name, DataType Dt, object DefaultValue, string? DisplayName)
{
	internal string _ActualDisplayName => DisplayName ?? Name;
}

/// <summary>
/// A field containing an integer
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="Min">Minimum integer value</param>
/// <param name="Max">Maximum integer value</param>
/// <param name="DefaultInt">Default value</param>
/// <param name="DisplayName">How the field's name should appear to end user. Defaults to Name.</param>
public sealed record EIntField(string Name, int Min, int Max, int DefaultInt, string? DisplayName = null) : EffectField(Name, DataType.Int, DefaultInt, DisplayName);
/// <summary>
/// A field containing a float
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="Min">Minimum value</param>
/// <param name="Max">Maximum value</param>
/// <param name="Step">Increment for the slider</param>
/// <param name="DefaultFloat">Default value</param>
/// <param name="DisplayName">How the field's name should appear to end user. Defaults to Name.</param>
public sealed record EFloatField(string Name, float Min, float Max, float Step, float DefaultFloat, string? DisplayName = null) : EffectField(Name, DataType.Float, DefaultFloat, DisplayName);

/// <summary>
/// A field containing a boolean
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="DefaultBool">Default value</param>
/// <param name="DisplayName">How the field's name should appear to end user. Defaults to Name.</param>
public sealed record EBoolField(string Name, bool DefaultBool, string? DisplayName = null) : EffectField(Name, DataType.Bool, DefaultBool, DisplayName);
/// <summary>
/// A field containing a string
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="DefaultString"></param>
/// <param name="DisplayName">How the field's name should appear to end user. Defaults to Name.</param>
public sealed record EStringField(string Name, string DefaultString, string? DisplayName = null) : EffectField(Name, DataType.String, DefaultString ?? "", DisplayName);
