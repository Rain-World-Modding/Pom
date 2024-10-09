using UnityEngine;
using DevInterface;

namespace Pom;
#pragma warning disable CS1591
public class TextInputSlider : Slider, IDevUISignals
{
	public float ActualValue
	{
		get => _actualValue;
		set
		{
			_actualValue = value;

			if (valueRounding >= 0)
			{ _actualValue = (float)Math.Round(_actualValue, 2); }

			(subNodes[1] as StringControl)!.ActualValue = _actualValue.ToString();
			Refresh();
		}
	}

	protected float _actualValue = 0;

	public float defaultValue { get; init; } = 0f;

	public float minValue { get; init; } = 0f;

	public float maxValue { get; init; } = 1f;

	public int valueRounding { get; init; } = -1;

	public int displayRounding { get; init; } = 0;

	private float stringWidth;
	public TextInputSlider(string IDstring, DevUINode parentNode, Vector2 pos, string title, bool inheritButton, float titleWidth, float stringWidth = 16) : base(parentNode.owner, IDstring, parentNode, pos, title, inheritButton, titleWidth)
	{
		//swap out old value display with StringControl
		subNodes[1].ClearSprites();
		subNodes[1] = new StringControl(owner, IDstring, this, new Vector2(titleWidth + 10f, 0f), inheritButton ? stringWidth + 26f : stringWidth, _actualValue.ToString(), TextIsFloatIsInRange);

		if (inheritButton && subNodes[2] is PositionedDevUINode node)
		{ node.Move(new Vector2(node.pos.x + (stringWidth - 16f), node.pos.y)); }

		this.stringWidth = stringWidth;
	}
	private new float SliderStartCoord
	{
		get
		{
			if (!inheritButton)
			{
				return titleWidth + 10f + stringWidth + 4f;
			}
			return titleWidth + 10f + 26f + stringWidth + 4f + 34f;
		}
	}

	public SliderNub sliderNub => (SliderNub)subNodes[inheritButton ? 3 : 2];
	public override void Update()
	{
		base.Update();
		if (owner != null && sliderNub.held)
		{
			NubDragged(Mathf.InverseLerp(absPos.x + SliderStartCoord, absPos.x + SliderStartCoord + 92f, owner.mousePos.x + sliderNub.mousePosOffset));
		}
	}

	public new void RefreshNubPos(float nubPos)
	{
		sliderNub.Move(new Vector2(Mathf.Lerp(SliderStartCoord, SliderStartCoord + 92f, nubPos), 0f));
	}

	public override void Refresh()
	{
		base.Refresh();

		if (Pom.ManagedStringControl.activeStringControl != subNodes[1])
		{
			string str = "";
			if (_actualValue == defaultValue && inheritButton) str = "<D>";
			str += " ";
			if (displayRounding >= 0) str += Math.Round(_actualValue, displayRounding).ToString();
			else str += _actualValue.ToString();

			NumberText = str;
		}

		RefreshNubPos(Mathf.Clamp(Mathf.InverseLerp(minValue, maxValue, _actualValue), 0f, 1f));
		MoveSprite(0, absPos + new Vector2(SliderStartCoord, 0f));
		MoveSprite(1, absPos + new Vector2(SliderStartCoord, 7f));
	}

	public override void NubDragged(float nubPos)
	{
		ActualValue = Mathf.Lerp(minValue, maxValue, nubPos);

		this.SendSignal(SliderUpdate, this, "");
	}

	public override void ClickedResetToInherent()
	{
		ActualValue = defaultValue;
	}

	public new void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender.IDstring == "Inherit_Button")
		{
			ClickedResetToInherent();
		}

		else if (sender.IDstring == IDstring && type == StringControl.StringFinish)
		{
			ActualValue = float.Parse((subNodes[1] as StringControl)!.ActualValue);
		}

		this.SendSignal(SliderUpdate, this, "");
	}

	public static readonly DevUISignalType SliderUpdate = new("SliderUpdate", true);

	public static bool TextIsFloatIsInRange(StringControl self, string value)
	{
		var slider = self.parentNode as TextInputSlider;
		return float.TryParse(value, out var result) && (slider == null || (slider.minValue <= result && result <= slider.maxValue));
	}
}
