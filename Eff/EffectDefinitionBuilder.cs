namespace Eff;

public sealed class EffectDefinitionBuilder
{
	private DevInterface.RoomSettingsPage.DevEffectsCategories? _category;
	private string _name;
	private Func<Room, EffectExtraData, FirstTimeRealized, UpdatableAndDeletable?>? _UADFactory;
	private Dictionary<string, EffectField> _fields = new();


	public EffectDefinitionBuilder(string name)
	{
		this._name = name;
	}

	public EffectDefinitionBuilder SetCategory(string categoryName)
	{
		this._category = new(categoryName, true);
		return this;
	}
	public EffectDefinitionBuilder AddField(EffectField field)
	{
		_fields[field.Name] = field;
		return this;
	}
	public EffectDefinitionBuilder SetUADFactory(Func<Room, EffectExtraData, FirstTimeRealized, UpdatableAndDeletable?>? factory)
	{
		_UADFactory = factory;
		return this;
	}
	public EffectDefinition Build()
	{

		return new(_category, _name, _UADFactory, new(_fields));
	}

}
