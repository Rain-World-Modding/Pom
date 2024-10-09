using UnityEngine;
using DevInterface;

namespace Pom;
#pragma warning disable CS1591

public class ColorBox : DevUILabel
{
	public ColorBox(DevUI owner, DevUINode parentNode, Vector2 pos, string idstring, float width, float height, Color color) : base(owner, idstring, parentNode, pos, width, "")
	{
		fSprites[0].scaleY = height;
		fSprites[0].color = color;
	}

	public Color Color
	{
		get => fSprites[0].color;
		set => fSprites[0].color = value;
	}
}

public class ColorButton(DevUI owner, DevUINode parentNode, Vector2 pos, string idstring, float width, float height, Color color)
	: ColorBox(owner, parentNode, pos, idstring, width, height, color)
{
	private bool down = false;

	public override void Update()
	{
		base.Update();
		if (owner != null && owner.mouseClick && MouseOver)
		{
			this.down = true;
			this.SendSignal(DevUISignalType.ButtonClick, this, "colorButton");
			Refresh();
		}
		if (down && (!MouseOver || owner == null || !owner.mouseDown))
		{
			down = false;
			Refresh();
		}
	}
	public override void Refresh()
	{
		base.Refresh();
		fSprites[0].alpha = !down ? 0.5f : 1f;
	}
}
