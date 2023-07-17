using DevInterface;
using UnityEngine;

namespace Eff;

internal class CustomBoolButton : DevInterface.Button
{
    public (BoolField field, Cached<bool> cache) Data { get; }
	public RoomSettings.RoomEffect Effect { get; }

	public CustomBoolButton(
		DevUI owner,
		string IDstring,
		DevUINode parentNode,
		Vector2 pos,
		(BoolField field, Cached<bool> cache) data,
		RoomSettings.RoomEffect effect
		) : base(
			owner,
			IDstring,
			parentNode,
			pos,
			Eff.BOOL_BUTTON_WIDTH,
			data.cache.val.ToString())
	{
		Data = data;
		Effect = effect;
	}
	public override void Clicked()
	{
		base.Clicked();
		if (Effect.inherited) return;
        Data.cache.val = !Data.cache.val;
        this.Text = Data.cache.val.ToString();
	}

	
}