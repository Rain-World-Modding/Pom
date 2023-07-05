using DevInterface;
using UnityEngine;

using static Pom.Pom.ManagedStringControl;

namespace Eff;

public class CustomStringPanel : PositionedDevUINode
{
	private bool _clickedLastUpdate;
	private DevUILabel _labelValue;
	public (StringField field, Cached<string> cache) Data { get; }
	public RoomSettings.RoomEffect Effect { get; }

	public CustomStringPanel(
		DevUI owner,
		string IDstring,
		DevUINode parentNode,
		Vector2 pos,
		float width,
		(StringField field, Cached<string> cache) data,
		RoomSettings.RoomEffect effect
		) : base(
			owner,
			IDstring,
			parentNode,
			pos)
	{
		Data = data;
		Effect = effect;

		//subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0, 0), Eff.DEVUI_TITLE_WIDTH, Data.field.Name));
		_labelValue = new DevUILabel(owner, "Text", this, new Vector2(0, 0), width, Data.cache.val);
		subNodes.Add(_labelValue);
	}
	public override void Update()
	{
		if (Effect.inherited)
		{
			goto done;
		}
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
						_labelValue.Text = _labelValue.Text.Substring(0, _labelValue.Text.Length - 1);
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
	}

	private void LoseFocusAndWriteCached()
	{
		WriteCached();
		activeStringControl = null;
		_labelValue.fLabels[0].color = Color.black;
		plog.LogDebug("Setting new string value from label contents");
	}

	private void WriteCached()
	{
		Data.cache.val = _labelValue.Text;
	}
}