using DevInterface;
using UnityEngine;

namespace Eff;

public class CustomBoolButton : DevInterface.Button
{
    public (BoolField field, Cached<bool> cache) Data { get; }
	public CustomBoolButton(
		DevUI owner,
		string IDstring,
		DevUINode parentNode,
		Vector2 pos,
		(BoolField field, Cached<bool> cache) data
		) : base(
			owner,
			IDstring,
			parentNode,
			pos,
			Eff.BOOL_BUTTON_WIDTH,
			data.cache.val.ToString())
	{
		Data = data;
	}
	public override void Clicked()
	{
		base.Clicked();
        Data.cache.val = !Data.cache.val;
        this.Text = Data.cache.val.ToString();
	}

	
}