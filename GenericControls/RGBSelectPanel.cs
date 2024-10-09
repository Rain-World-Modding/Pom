using UnityEngine;
using DevInterface;
using static Pom.Pom;

namespace Pom;
#pragma warning disable CS1591

public class RGBSelectPanel : Panel, IDevUISignals
{
	private readonly TextInputSlider[] RGBSliders;
	private readonly TextInputSlider[] HSLSliders;
	private readonly StringControl Hex;
	private readonly ColorBox colorBox;
	private Color _actualValue = new(0f, 0f, 0f);
	public Color ActualValue
	{
		get => _actualValue;
		set
		{
			_actualValue = value;
			UpdateHSLSliders();
			UpdateRGBSliders();
			UpdateHex();
			Refresh();
		}
	}

	public RGBSelectPanel(DevUI owner, DevUINode parentNode, Vector2 pos, string idstring, string title, Color color)
		: base(owner, idstring, parentNode, pos, new Vector2(305f, 200f), title)
	{
		RGBSliders = new TextInputSlider[3];
		HSLSliders = new TextInputSlider[3];
		//^ all set in PlaceNodes
		Vector2 elementPos = new Vector2(size.x - 185f, size.y - 25f);
		AddSlider(ref RGBSliders[0], "R_RGB", " R", elementPos);

		elementPos.y -= 20f;
		AddSlider(ref RGBSliders[1], "G_RGB", " G", elementPos);

		elementPos.y -= 20f;
		AddSlider(ref RGBSliders[2], "B_RGB", " B", elementPos);

		elementPos.y -= 40f;
		Hex = new StringControl(owner, "HEX", this, elementPos + new Vector2(77f, 0f), 92f, "000000", StringControl.TextIsColor);
		subNodes.Add(Hex);
		subNodes.Add(new DevUILabel(owner, "HEX_LABEL", this, elementPos, 60f, "HEX"));
		elementPos.y -= 40f;

		AddSlider(ref HSLSliders[0], "H_HSL", " H", elementPos);

		elementPos.y -= 20f;
		AddSlider(ref HSLSliders[1], "S_HSL", " S", elementPos);

		elementPos.y -= 20f;
		AddSlider(ref HSLSliders[2], "L_HSL", " L", elementPos);

		colorBox = new ColorBox(owner, this, new Vector2(5f, size.y - 69f), "colorbox", 64f, 64f, color);
		subNodes.Add(colorBox);

		elementPos = new Vector2(5f, 54f);
		foreach ((string name, string namedColor) in ColorField.namedColors)
		{
			subNodes.Add(new ColorButton(owner, this, elementPos, name, 16, 16, RWCustom.Custom.hexToColor(namedColor)));
			elementPos.y -= 20f;
			if (elementPos.y < 0f)
			{
				elementPos.x += 20f;
				elementPos.y = 54f;
			}
		}

		ActualValue = color;
	}

	public void AddSlider(ref TextInputSlider slider, string name, string display, Vector2 pos)
	{
		if (slider != null && subNodes.Contains(slider)) subNodes.Remove(slider);
		slider = new TextInputSlider(name, this, pos, display, false, 20f, 32f) { valueRounding = 2, displayRounding = 2 };
		subNodes.Add(slider);
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (RGBSliders.Contains(sender))
		{
			_actualValue = new Color(RGBSliders[0].ActualValue, RGBSliders[1].ActualValue, RGBSliders[2].ActualValue);
			UpdateHSLSliders();
			UpdateHex();
		}
		else if (HSLSliders.Contains(sender))
		{
			_actualValue = RWCustom.Custom.HSL2RGB(HSLSliders[0].ActualValue, HSLSliders[1].ActualValue, HSLSliders[2].ActualValue);
			UpdateRGBSliders();
			UpdateHex();
		}

		else if (sender == Hex)
		{
			_actualValue = RWCustom.Custom.hexToColor(Hex.ActualValue);
			UpdateHSLSliders();
			UpdateRGBSliders();
		}

		else if (sender is ColorButton cb)
		{
			ActualValue = cb.Color;
		}

		{ this.SendSignal(type, this, message); }
	}

	public override void Refresh()
	{
		base.Refresh();
		colorBox.Color = ActualValue;
	}

	public void UpdateRGBSliders()
	{
		RGBSliders[0].ActualValue = ActualValue.r;
		RGBSliders[0].Refresh();
		RGBSliders[1].ActualValue = ActualValue.g;
		RGBSliders[1].Refresh();
		RGBSliders[2].ActualValue = ActualValue.b;
		RGBSliders[2].Refresh();
	}

	public void UpdateHSLSliders()
	{
		Vector3 HSLpos = RWCustom.Custom.RGB2HSL(ActualValue);
		HSLSliders[0].ActualValue = HSLpos.x;
		HSLSliders[0].Refresh();
		HSLSliders[1].ActualValue = HSLpos.y;
		HSLSliders[1].Refresh();
		HSLSliders[2].ActualValue = HSLpos.z;
		HSLSliders[2].Refresh();
	}

	public void UpdateHex()
	{
		Hex.ActualValue = RWCustom.Custom.colorToHex(ActualValue);
	}

	public class RGBSelectButton : Button, IDevUISignals
	{
		public bool recolor { get; init; } = false;
		public bool hexDisplay { get; init; } = true;
		public RGBSelectButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text,
			Color color, string panelName)
			: base(owner, IDstring, parentNode, pos, width, text)
		{
			actualValue = color;
			RGBSelectPanel = null;
			this.panelName = panelName;

			if (hexDisplay)
			{ Text = RWCustom.Custom.colorToHex(actualValue); }
		}

		public override void Update()
		{
			base.Update();
			if (recolor)
			{ fSprites[0].color = Color.Lerp(actualValue, colorA, 0.5f); }
		}

		public override void Clicked()
		{
			//sending signal first in case values wanna be changed
			base.Clicked();
			if (RGBSelectPanel != null)
			{
				subNodes.Remove(RGBSelectPanel);
				RGBSelectPanel.ClearSprites();
				RGBSelectPanel = null;
			}
			else
			{
				Vector2 setPos = panelPos - absPos;
				RGBSelectPanel = new RGBSelectPanel(owner, this, setPos, IDstring + "_Panel", panelName, actualValue);
				subNodes.Add(RGBSelectPanel);
			}
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (RGBSelectPanel != null && sender == RGBSelectPanel)
			{
				actualValue = RGBSelectPanel.ActualValue;
				if (hexDisplay)
				{ Text = RWCustom.Custom.colorToHex(actualValue); }
			}

			//send signal up
			this.SendSignal(DevUISignalType.ButtonClick, this, message);
		}

		public RGBSelectPanel? RGBSelectPanel;

		public string panelName;

		public Color actualValue;

		public Vector2 panelPos = new(420f, 280f);
	}
}
