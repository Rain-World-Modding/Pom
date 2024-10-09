using UnityEngine;
using DevInterface;

namespace Pom;
#pragma warning disable CS1591
public class ItemSelectPanel : Panel, IDevUISignals
{

	const string BACKBUTTON = "BackPage99289..?/~";
	const string NEXTBUTTON = "NextPage99289..?/~";

	private readonly PanelSelectButton ownerButton;

	private bool goNext = false;

	private bool goPrev = false;

	private readonly float buttonWidth;

	private readonly int columns;

	private readonly int perpage;

	private int currentOffset;


	public ItemSelectPanel(
		DevUI owner,
		PanelSelectButton parentNode,
		Vector2 pos,
		string idstring,
		string title,
		Vector2 size = default,
		float buttonWidth = 145f,
		int columns = 2)
		: base(
			owner,
			idstring,
			parentNode,
			pos,
			size == default ? new Vector2(305f, 420f) : size,
			title)
	{
		ownerButton = parentNode;
		this.buttonWidth = buttonWidth;
		this.columns = columns;

		currentOffset = 0;
		perpage = (int)((this.size.y - 60f) / 20f * columns);
		PopulateItems(currentOffset);
	}
	public void PopulateItems(int offset)
	{
		IEnumerable<string> items = ownerButton.GetItems(offset);
		if (items.Count() == 0) return;
		currentOffset = offset;
		foreach (DevUINode devUINode in subNodes)
		{
			devUINode.ClearSprites();
		}

		subNodes.Clear();
		var intVector = new RWCustom.IntVector2(0, 0);

		foreach (string item in items)
		{
			var currentOption = new Button(owner, IDstring + "Button99289_" + item, this, new Vector2(5f + intVector.x * (buttonWidth + 5f), size.y - 25f - 20f * intVector.y), buttonWidth, item);
			subNodes.Add(currentOption);
			intVector.y++;
			if (intVector.y >= (int)Mathf.Floor(perpage / columns))
			{
				intVector.x++;
				intVector.y = 0;
			}
		}

		float pageButtonWidth = (size.x - 15f) / 2f;

		subNodes.Add(new Button(owner, IDstring + BACKBUTTON, this, new Vector2(5f, size.y - 25f - 20f * (perpage / columns + 1f)), pageButtonWidth, "Previous"));
		subNodes.Add(new Button(owner, IDstring + NEXTBUTTON, this, new Vector2(size.x - 5f - pageButtonWidth, size.y - 25f - 20f * (perpage / columns + 1f)), pageButtonWidth, "Next"));
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		//can't modify subnodes during Signal, as it is called during a loop through all subnodes
		if (sender.IDstring == IDstring + BACKBUTTON)
		{ goPrev = true; }

		else if (sender.IDstring == IDstring + NEXTBUTTON)
		{ goNext = true; }

		else { ownerButton.Signal(type, sender, message); }
	}

	public override void Update()
	{
		if (goNext)
		{ PopulateItems(currentOffset + 1); goNext = false; }

		if (goPrev)
		{ PopulateItems(currentOffset - 1); goPrev = false; }

		base.Update();
	}
	public class PanelSelectButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text, string[] allItems)
		: Button(owner, IDstring, parentNode, pos, width, text), IDevUISignals
	{
		public Vector2 panelPos { get; init; } = new Vector2(200f, 15f);

		public Vector2 panelSize { get; init; } = new Vector2(305f, 420f);

		public string panelName { get; init; } = "";

		public float panelButtonWidth { get; init; } = 145f;

		public int panelColumns { get; init; } = 2;

		public int ItemsPerPage => (int)((panelSize.y - 60f) / 20f * panelColumns);

		public string[] allItems = allItems;

		protected ItemSelectPanel? itemSelectPanel = null;

		protected string _actualValue = "";

		public string ActualValue
		{
			get => _actualValue;
			set
			{
				_actualValue = value;
				Text = value;
				Refresh();
			}
		}

		public override void Clicked()
		{
			if (itemSelectPanel != null)
			{
				RemovePanel();
			}
			else
			{
				Vector2 setPos = panelPos - absPos;
				itemSelectPanel = new ItemSelectPanel(owner, this, setPos, IDstring + "_Panel", IDstring, panelSize, panelButtonWidth, panelColumns);
				subNodes.Add(itemSelectPanel);
			}
			this.Refresh();
		}

		/// <summary>
		/// detects if signal is from sub-button and if so, processes the actual item value
		/// </summary>
		public bool IsSubButtonID(string IDstring, out string subButtonID)
		{
			subButtonID = "";
			if (itemSelectPanel != null && IDstring.StartsWith(itemSelectPanel.IDstring + "Button99289_"))
			{
				subButtonID = IDstring.Remove(0, (itemSelectPanel.IDstring + "Button99289_").Length);
				return true;
			}

			return false;
		}
		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (IsSubButtonID(sender.IDstring, out var item))
			{
				ActualValue = item;
				RemovePanel();
			}

			//send signal up
			this.SendSignal(DevUISignalType.ButtonClick, this, message);
		}

		public void RemovePanel()
		{
			parentNode.subNodes.Remove(itemSelectPanel);
			itemSelectPanel?.ClearSprites();
			itemSelectPanel = null;
		}

		public virtual IEnumerable<string> GetItems(int page)
		{
			for (int i = (int)Math.Max(0f, page * ItemsPerPage); i < Math.Min(allItems.Count(), (page + 1) * ItemsPerPage); i++)
			{
				yield return allItems[i];
			}
		}
	}
}

