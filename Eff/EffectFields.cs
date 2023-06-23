namespace Pom.Eff;

public abstract record EffectField(string Name, DataType Dt, object DefaultValue);

public record IntField(string Name, int Min, int Max, int DefaultInt) : EffectField(Name, DataType.Int, DefaultInt);
public record FloatField(string Name, float Min, float Max, float Step, float DefaultFloat) : EffectField(Name, DataType.Float, DefaultFloat);
public record BoolField(string Name, bool DefaultBool) : EffectField(Name, DataType.Float, DefaultBool);
public record StringField(string Name, string DefaultString) : EffectField(Name, DataType.String, DefaultString ?? "");
