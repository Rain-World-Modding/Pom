using DevInterface;
using UnityEngine;

namespace Eff;

public class CustomIntButton : DevInterface.Button
{
	private readonly BType type;
	private readonly (IntField field, Cached<int> cache) _data;
	private readonly DevUILabel _valueLabel;

	public CustomIntButton(
		DevUI owner,
		string IDstring,
		DevUINode parentNode,
		Vector2 pos,
		BType type,
		(IntField field, Cached<int> cache) data,
		DevUILabel valueLabel
		) : base(
			owner,
			IDstring,
			parentNode,
			pos,
			Eff.INT_BUTTON_WIDTH,
			type switch { BType.Decrement => " - ", BType.Increment => " + ", _ => "???" })
	{
		this.type = type;
		this._data = data;
		this._valueLabel = valueLabel;
	}

	public override void Clicked()
	{
		base.Clicked();
        int newval = this._data.cache.val + type switch { BType.Decrement => -1, BType.Increment => +1, _ => 0 };
        if (newval > this._data.field.Max) {
            newval = this._data.field.Min;
        }
        else if (newval < this._data.field.Min) {
            newval = this._data.field.Max;
        }
        this._data.cache.val = newval;
		this._valueLabel.Text = this._data.cache.val.ToString();
	}

	public enum BType
	{
		Increment,
		Decrement
	}
}