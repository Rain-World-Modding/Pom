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
		(FloatField field, Cached<float> cache) data,
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
		Data = data;
	}

	public (FloatField field, Cached<float> cache) Data { get; }
	public RoomSettings.RoomEffect Effect { get; }

	public override void Refresh()
	{
		base.Refresh();
		if (Data.cache is null || Data.field is null)
		{
			plog.LogError(Data);
			return;
		}
		this.NumberText = Data.cache.val.ToString();
		float amount = Mathf.InverseLerp(Data.field.Min, Data.field.Max, Data.cache.val);
		RefreshNubPos(amount);
	}
	public override void NubDragged(float nubPos)
	{
		if (this.Effect.inherited)
		{
			this.Refresh();
			return;
		}
		float unroundedVal = Mathf.Lerp(Data.field.Min, Data.field.Max, nubPos);
		this.Data.cache.val = unroundedVal - unroundedVal % this.Data.field.Step;
		this.Refresh();
		//base.NubDragged(nubPos);
	}
}