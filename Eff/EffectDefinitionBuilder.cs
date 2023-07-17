namespace Eff;

/// <summary>
/// Builder class for creating an effect definition.
/// </summary>
public sealed class EffectDefinitionBuilder
{
	private bool _built = false;
	private string? _category;
	private string _name;
	private Func<Room, EffectExtraData, FirstTimeRealized, UpdatableAndDeletable?>? _UADFactory;
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
	/// Adds a new field to the definition. If there was another with the same name, it gets replaces.
	/// </summary>
	/// <param name="field">New field.</param>
	/// <returns>Itself.</returns>
	public EffectDefinitionBuilder AddField(EffectField field)
	{
		ThrowIfBuilt();
		_fields[field.Name] = field;
		return this;
	}
	/// <summary>
	/// Sets a callback that will be used to add UpdatableAndDeletables to rooms where this effect is enabled.
	/// <para/>
	/// Callback's arguments are: current room, <see cref="global::Eff.EffectExtraData"/> object which holds field values,
	/// an <see cref="FirstTimeRealized"/> enum which indicates whether the room is being realized for the first time.
	/// Callback should return the new UpdatableAndDeletable, or null if the effect should not spawn an object in this room.
	/// </summary>
	/// <param name="factory"></param>
	/// <returns></returns>
	public EffectDefinitionBuilder SetUADFactory(Func<Room, EffectExtraData, FirstTimeRealized, UpdatableAndDeletable?>? factory)
	{
		ThrowIfBuilt();
		_UADFactory = factory;
		return this;
	}
	public void Register() {
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
}
