using DevInterface;
using UnityEngine;

using static Pom.Pom.ManagedStringControl;

namespace EffExt;

internal class CustomStringPanel : PositionedDevUINode
{
	private bool _clickedLastUpdate;
	private DevUILabel _labelValue;
	private FSprite[] outlineSprites;

	public (EStringField field, Cached<string> cache) Data { get; }
	public RoomSettings.RoomEffect Effect { get; }

	public CustomStringPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, (EStringField field, Cached<string> cache) data, RoomSettings.RoomEffect effect) 
		: base(owner, IDstring, parentNode, pos)
	{
		Data = data;
		Effect = effect;

		//subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0, 0), Eff.DEVUI_TITLE_WIDTH, Data.field.Name));
		_labelValue = new DevUILabel(owner, "Text", this, new Vector2(0, 0), width, Data.cache.Value);
		subNodes.Add(_labelValue);

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
	}

	public override void Refresh()
	{
		base.Refresh();

		// Update outline sprites
		outlineSprites[0].SetPosition(_labelValue.absPos - Vector2.one * 0.99f);
		outlineSprites[0].scaleX = _labelValue.size.x + 2;
		outlineSprites[1].SetPosition(_labelValue.absPos - Vector2.one * 0.99f);
		outlineSprites[1].scaleY = _labelValue.size.y + 2;
		outlineSprites[2].SetPosition(_labelValue.absPos + new Vector2(0f, _labelValue.size.y) + Vector2.one * 0.01f);
		outlineSprites[2].scaleX = _labelValue.size.x + 1;
		outlineSprites[3].SetPosition(_labelValue.absPos + new Vector2(_labelValue.size.x, 0f) + Vector2.one * 0.01f);
		outlineSprites[3].scaleY = _labelValue.size.y + 1;
	}

	public override void Update()
	{
		if (Effect.inherited)
		{
			goto done;
		}

		// Typing logic
		if (owner.mouseClick && !_clickedLastUpdate)
		{
			if (_labelValue.MouseOver && activeStringControl != this)
			{
				// replace whatever instance/null that was focused
				activeStringControl = this;
				_labelValue.fLabels[0].color = new Color(0.1f, 0.4f, 0.2f);
			}
			else if (activeStringControl == this)
			{
				// focus lost
				LoseFocusAndWriteCached();
			}
		}

		if (activeStringControl == this)
		{
			foreach (char c in Input.inputString)
			{
				switch (c)
				{
				case '\b':
					if (_labelValue.Text.Length != 0)
					{
						_labelValue.Text = _labelValue.Text[..^1];
						WriteCached();
					}
					break;
				case '\n':
				case '\r':
					LoseFocusAndWriteCached();
					break;
				default:
					_labelValue.Text += c;
					WriteCached();
					break;
				}
			}
		}

	done:
		_clickedLastUpdate = owner.mouseClick && _labelValue.MouseOver;

		// Update outline sprite visibility
		bool focused = activeStringControl == this;
		foreach (FSprite sprite in outlineSprites)
		{
			sprite.isVisible = focused;
		}
	}

	private void LoseFocusAndWriteCached()
	{
		WriteCached();
		activeStringControl = null;
		_labelValue.fLabels[0].color = Color.black;
		LogDebug("Setting new string value from label contents");
	}

	private void WriteCached()
	{
		Data.cache.Value = _labelValue.Text;
	}
}
