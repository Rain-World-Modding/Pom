using DevInterface;
using UnityEngine;

namespace EffExt;

internal class CustomIntButton : DevInterface.Button
{
	public BType Btype { get; }
	public (EIntField field, Cached<int> cache) Data { get; }
	public RoomSettings.RoomEffect Effect { get; }

	private readonly DevUILabel _valueLabel;

	public CustomIntButton(
		DevUI owner,
		string IDstring,
		DevUINode parentNode,
		Vector2 pos,
		BType Btype,
		(EIntField field, Cached<int> cache) data,
		DevUILabel valueLabel,
		RoomSettings.RoomEffect effect
		) : base(
			owner,
			IDstring,
			parentNode,
			pos,
			Eff.INT_BUTTON_WIDTH,
			Btype switch { BType.Decrement => " - ", BType.Increment => " + ", _ => "???" })
	{
		this.Btype = Btype;
		this.Data = data;
		this._valueLabel = valueLabel;
		Effect = effect;
	}

	public override void Clicked()
	{
		base.Clicked();
		if (Effect.inherited) return;
		int newval = this.Data.cache.Value + Btype switch { BType.Decrement => -1, BType.Increment => +1, _ => 0 };
		if (newval > this.Data.field.Max)
		{
			newval = this.Data.field.Min;
		}
		else if (newval < this.Data.field.Min)
		{
			newval = this.Data.field.Max;
		}
		this.Data.cache.Value = newval;
		this._valueLabel.Text = this.Data.cache.Value.ToString();
	}

	public enum BType
	{
		Increment,
		Decrement
	}
}