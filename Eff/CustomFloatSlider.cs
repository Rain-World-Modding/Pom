using UnityEngine;
using DevInterface;

namespace Eff;
public class CustomFloatSlider : Slider
{
	public CustomFloatSlider(
		DevUI owner,
		string IDstring,
		DevUINode parentNode,
		Vector2 pos,
		string title,
		(FloatField field, Cached<float> cache) value,
		RoomSettings.RoomEffect effect) : base(
			owner,
			IDstring,
			parentNode,
			pos,
			title,
			false,
			Eff.DEVUI_TITLE_WIDTH)
	{
		Effect = effect;
		Value = value;
	}

	public (FloatField field, Cached<float> cache) Value { get; }
	public RoomSettings.RoomEffect Effect { get; }

	public override void Refresh()
	{
		base.Refresh();
		if (Value.cache is null || Value.field is null)
		{
			plog.LogError(Value);
			return;
		}
		this.NumberText = Value.cache.val.ToString();
		float amount = Mathf.InverseLerp(Value.field.Min, Value.field.Max, Value.cache.val);
		RefreshNubPos(amount);
	}
	public override void NubDragged(float nubPos)
	{
		if (this.Effect.inherited)
		{
			this.Refresh();
			return;

		}

		float unroundedVal = Mathf.Lerp(Value.field.Min, Value.field.Max, nubPos);
		this.Value.cache.val = unroundedVal - unroundedVal % this.Value.field.Step;
		this.Refresh();
		//base.NubDragged(nubPos);
	}
}