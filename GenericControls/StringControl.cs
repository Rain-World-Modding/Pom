using DevInterface;
using UnityEngine;
using static Pom.Pom;

namespace Pom;
#pragma warning disable CS1591

public class StringControl : DevUILabel
{
	protected FSprite[] outlineSprites;

	public StringControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text, IsTextValid del) : base(owner, IDstring, parentNode, pos, width, text)
	{
		outlineSprites = new FSprite[4];
		for (int i = 0; i < outlineSprites.Length; i++)
		{
			outlineSprites[i] = new FSprite("pixel")
			{
				anchorX = 0f,
				anchorY = 0f,
				color = Color.white,
				isVisible = false,
			};
			fSprites.Add(outlineSprites[i]);
			if (owner != null)
			{
				Futile.stage.AddChild(outlineSprites[i]);
			}
		}

		isTextValid = del;
		actualValue = text; //shut up compiler
		ActualValue = text;

	}

	protected bool clickedLastUpdate = false;


	protected string actualValue;

	public string ActualValue
	{
		get => actualValue;
		set
		{
			actualValue = value;
			Text = value;
			Refresh();
		}
	}

	public override void Refresh()
	{
		base.Refresh();

		// Update outline sprites
		outlineSprites[0].SetPosition(absPos - Vector2.one * 0.99f);
		outlineSprites[0].scaleX = size.x + 2;
		outlineSprites[1].SetPosition(absPos - Vector2.one * 0.99f);
		outlineSprites[1].scaleY = size.y + 2;
		outlineSprites[2].SetPosition(absPos + new Vector2(0f, size.y) + Vector2.one * 0.01f);
		outlineSprites[2].scaleX = size.x + 1;
		outlineSprites[3].SetPosition(absPos + new Vector2(size.x, 0f) + Vector2.one * 0.01f);
		outlineSprites[3].scaleY = size.y + 1;
	}

	public override void Update()
	{
		if (owner.mouseClick && !clickedLastUpdate)
		{
			if (MouseOver && Pom.ManagedStringControl.activeStringControl != this)
			{
				// replace whatever instance/null that was focused
				Text = actualValue;
				Pom.ManagedStringControl.activeStringControl = this;
				fLabels[0].color = new Color(0.1f, 0.4f, 0.2f);
			}
			else if (Pom.ManagedStringControl.activeStringControl == this)
			{
				// focus lost
				TrySetValue(Text, true);
				Pom.ManagedStringControl.activeStringControl = null;
				fLabels[0].color = Color.black;
			}

			clickedLastUpdate = true;
		}
		else if (!owner.mouseClick)
		{
			clickedLastUpdate = false;
		}

		if (Pom.ManagedStringControl.activeStringControl == this)
		{
			foreach (char c in Input.inputString)
			{
				if (c == '\b')
				{
					if (Text.Length != 0)
					{
						Text = Text.Substring(0, Text.Length - 1);
						TrySetValue(Text, false);
					}
				}
				else if (c == '\n' || c == '\r')
				{
					// should lose focus
					TrySetValue(Text, true);
					Pom.ManagedStringControl.activeStringControl = null;
					fLabels[0].color = Color.black;
				}
				else
				{
					Text += c;
					TrySetValue(Text, false);
				}
			}
		}

		// Update outline sprite visibility
		bool focused = ManagedStringControl.activeStringControl == this;
		foreach (FSprite sprite in outlineSprites)
		{
			sprite.isVisible = focused;
		}
	}

	public delegate bool IsTextValid(StringControl self, string value);

	public IsTextValid isTextValid;


	protected virtual void TrySetValue(string newValue, bool endTransaction)
	{
		Color outlineColor;
		if (isTextValid(this, newValue))
		{
			actualValue = newValue;
			fLabels[0].color = new Color(0.1f, 0.4f, 0.2f);
			outlineColor = Color.white;
			this.SendSignal(StringEdit, this, "");
		}
		else
		{
			fLabels[0].color = Color.red;
			outlineColor = Color.red;
		}
		foreach (FSprite sprite in outlineSprites)
		{
			sprite.color = outlineColor;
		}
		if (endTransaction)
		{
			Text = actualValue;
			fLabels[0].color = Color.black;
			Refresh();
			this.SendSignal(StringFinish, this, "");
		}
	}

	/// <summary>
	/// returns result of float.TryParse
	/// </summary>
	public static bool TextIsFloat(StringControl __, string value) => float.TryParse(value, out _);

	/// <summary>
	/// returns result of int.TryParse
	/// </summary>
	public static bool TextIsInt(StringControl __, string value) => (int.TryParse(value, out int i) && i.ToString() == value);


	/// <summary>
	/// returns true when text is valid hex color
	/// </summary>
	public static bool TextIsColor(StringControl __, string value)
	{
		try { Color color = RWCustom.Custom.hexToColor(value); return RWCustom.Custom.colorToHex(color) == value; }
		catch { return false; }
	}

	/// <summary>
	/// returns true when text is ExtEnum
	/// </summary>
	public static bool TextIsExtEnum<T>(StringControl __, string value) where T : ExtEnum<T> => ExtEnumBase.TryParse(typeof(T), value, false, out _);

	/// <summary>
	/// always returns true
	/// </summary>
	public static bool TextIsAny(StringControl __, string _) => true;

	/// <summary>
	/// returns true if text could be a filename
	/// </summary>
	public static bool TextIsValidFilename(StringControl __, string value) => value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

	/// <summary>
	/// signal value sent by <see cref="StringControl"/> when value is changed without editing
	/// </summary>
	public static readonly DevUISignalType StringEdit = new("StringEdit", true);
	/// <summary>
	/// signal value sent by <see cref="StringControl"/> when editing mode is finished
	/// </summary>
	public static readonly DevUISignalType StringFinish = new("StringFinish", true);
}

