namespace Eff;

/// <summary>
/// Base class for effect fields.
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="Dt">Type of data</param>
/// <param name="DefaultValue">Default value</param>
public abstract record EffectField(string Name, DataType Dt, object DefaultValue);

/// <summary>
/// A field containing an integer
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="Min">Minimum integer value</param>
/// <param name="Max">Maximum integer value</param>
/// <param name="DefaultInt">Default value</param>
public record IntField(string Name, int Min, int Max, int DefaultInt) : EffectField(Name, DataType.Int, DefaultInt);
/// <summary>
/// A field containing a float
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="Min">Minimum value</param>
/// <param name="Max">Maximum value</param>
/// <param name="Step">Increment for the slider</param>
/// <param name="DefaultFloat">Default value</param>
public record FloatField(string Name, float Min, float Max, float Step, float DefaultFloat) : EffectField(Name, DataType.Float, DefaultFloat);

/// <summary>
/// A field containing a boolean
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="DefaultBool">Default value</param>
public record BoolField(string Name, bool DefaultBool) : EffectField(Name, DataType.Float, DefaultBool);
/// <summary>
/// A field containing a string
/// </summary>
/// <param name="Name">Name of the field</param>
/// <param name="DefaultString"></param>
public record StringField(string Name, string DefaultString) : EffectField(Name, DataType.String, DefaultString ?? "");
