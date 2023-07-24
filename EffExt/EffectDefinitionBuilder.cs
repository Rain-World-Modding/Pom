namespace EffExt;

/// <summary>
/// Builder class for creating an effect definition.
/// </summary>
public sealed class EffectDefinitionBuilder
{
	private bool _built = false;
	private string? _category;
	private string _name;
	private UADFactory? _UADFactory;
	private EffectInitializer? _initializer;
	private Dictionary<string, EffectField> _fields = new();

	/// <summary>
	/// Creates a new blank instance.
	/// </summary>
	/// <param name="name"></param>
	public EffectDefinitionBuilder(string name)
	{
		this._name = name;
	}
	/// <summary>
	/// Sets the future definition's devtools category.
	/// </summary>
	/// <param name="categoryName">Name of the category.</param>
	/// <returns>Itself.</returns>
	public EffectDefinitionBuilder SetCategory(string categoryName)
	{
		ThrowIfBuilt();
		this._category = categoryName;//new(categoryName, true);
		return this;
	}
	/// <summary>
	/// Registers a new integer field.
	/// </summary>
	/// <param name="name">Name of the field.</param>
	/// <param name="minValue">Minimal value of the integer.</param>
	/// <param name="maxValue">Maximum value of the integer.</param>
	/// <param name="defaultValue">Default value of the integer.</param>
	/// <param name="displayName">How the field's name appears in DevUI.</param>
	/// <returns>Itself.</returns>
	public EffectDefinitionBuilder AddIntField(
		string name,
		int minValue,
		int maxValue,
		int defaultValue,
		string? displayName = null)
		=> AddField(new EIntField(name, minValue, maxValue, defaultValue, displayName));
	/// <summary>
	/// Registers a new float field.
	/// </summary>
	/// <param name="name">Name of the field.</param>
	/// <param name="minValue">Minimum value of the float.</param>
	/// <param name="maxValue">Maximum value of the float.</param>
	/// <param name="step">Increments the value is allowed to move by.</param>
	/// <param name="defaultValue">Default value of the float.</param>
	/// <param name="displayName">How the field's name appears in DevUI.</param>
	/// <returns>Itself.</returns>
	public EffectDefinitionBuilder AddFloatField(
		string name,
		float minValue,
		float maxValue,
		float step,
		float defaultValue,
		string? displayName = null)
		=> AddField(new EFloatField(name, minValue, maxValue, step, defaultValue, displayName));
	/// <summary>
	/// Registers a new boolean field.
	/// </summary>
	/// <param name="name">Field name.</param>
	/// <param name="defaultValue">Default value.</param>
	/// <param name="displayName">How the field's name appears in DevUI.</param>
	/// <returns>Itself.</returns>
	public EffectDefinitionBuilder AddBoolField(
		string name,
		bool defaultValue,
		string? displayName = null) 
		=> AddField(new EBoolField(name, defaultValue, displayName));
	/// <summary>
	/// Registers a new string field.
	/// </summary>
	/// <param name="name">Field name.</param>
	/// <param name="defaultValue">Default value.</param>
	/// <param name="displayName">How the field's name appears in DevUI.</param>
	/// <returns>Itself.</returns>
	public EffectDefinitionBuilder AddStringField(
		string name,
		string defaultValue,
		string? displayName = null)
		=> AddField(new EStringField(name, defaultValue, displayName));
	/// <summary>
	/// Adds a new field to the definition. If there was another with the same name, it gets replaces.
	/// </summary>
	/// <param name="field">New field.</param>
	/// <returns>Itself.</returns>
	public EffectDefinitionBuilder AddField(EffectField field)
	{
		ThrowIfBuilt();
		__ValidateDefaultValue(field.Dt, field.DefaultValue);
		if (!_fields.TryAdd(field.Name, field))
		{
			throw new ArgumentException($"There is already a field {field.Name} defined on effect definition builder {this._name}, cannot define twice.");
		}
		return this;
	}
	/// <summary>
	/// Sets a callback that will be used to add UpdatableAndDeletables to rooms where this effect is enabled.
	/// </summary>
	/// <param name="factory"></param>
	/// <returns></returns>
	public EffectDefinitionBuilder SetUADFactory(UADFactory factory)
	{
		ThrowIfBuilt();
		_UADFactory = factory;
		return this;
	}
	/// <summary>
	/// Sets a callback that runs on room load for this effect but does not produce an object.
	/// </summary>
	/// <param name="initializer"></param>
	/// <returns></returns>
	public EffectDefinitionBuilder SetEffectInitializer(EffectInitializer initializer)
	{
		ThrowIfBuilt();
		_initializer = initializer;
		return this;
	}
	/// <summary>
	/// Registers effect definition and closes this instance from further use.
	/// </summary>
	public void Register()
	{
		Eff.RegisterEffectDefinition(this._Build());
	}
	/// <summary>
	/// Creates an EffectDefinition from itself, and prevents itself from being used again.
	/// </summary>
	/// <returns>The resulting definition.</returns>
	private EffectDefinition _Build()
	{
		ThrowIfBuilt();
		this.Dispose();
		return new(
			(_category is null ? null : new(_category, true)),
			_name,
			_UADFactory,
			_initializer,
			new(_fields));
	}
	/// <summary>
	/// Prevents the instance from being used again.
	/// </summary>
	public void Dispose()
	{
		_built = true;
	}
	private void ThrowIfBuilt()
	{
		if (_built) throw new ObjectDisposedException(this.ToString(), "This EffectDefinitionBuilder has already been used");
	}
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
