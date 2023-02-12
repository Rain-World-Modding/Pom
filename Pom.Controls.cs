using DevInterface;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using System.Reflection;

using static Pom.Mod;

namespace Pom;

public static partial class Pom
{
	// An undocumented mess. Have a look around, find what suits you, maybe implement your own.
	// in commit d2dad8768371565bb9b538263ac0b0ac595913b7 there was a slider-button used for text before text input became a thing
	// an arrows-text combo would also be amazing for enums and ints wink wink

	public class ManagedVectorHandle : Handle // All-in-one super handle
	{
		protected readonly Vector2Field field;
		protected readonly ManagedData data;
		protected readonly Vector2Field.VectorReprType reprType;
		protected readonly int line = -1;
		protected readonly int circle = -1;
		protected readonly int[]? rect;

		public ManagedVectorHandle(Vector2Field field, ManagedData managedData, ManagedRepresentation repr, Vector2Field.VectorReprType reprType) : base(repr.owner, field.key, repr, managedData.GetValue<Vector2>(field.key))
		{
			this.field = field;
			this.data = managedData;
			this.reprType = reprType;
			switch (reprType)
			{
			case Vector2Field.VectorReprType.circle:
			case Vector2Field.VectorReprType.line:
				this.line = this.fSprites.Count;
				this.fSprites.Add(new FSprite("pixel", true));
				owner.placedObjectsContainer.AddChild(this.fSprites[line]);
				this.fSprites[line].anchorY = 0;
				if (reprType != Vector2Field.VectorReprType.circle)
					break;
				//case Vector2Field.VectorReprType.circle:
				this.circle = this.fSprites.Count;
				this.fSprites.Add(new FSprite("Futile_White", true));
				owner.placedObjectsContainer.AddChild(this.fSprites[circle]);
				this.fSprites[circle].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
				break;
			case Vector2Field.VectorReprType.rect:
				this.rect = new int[5];
				for (int i = 0; i < 5; i++)
				{
					this.rect[i] = this.fSprites.Count;
					this.fSprites.Add(new FSprite("pixel", true));
					owner.placedObjectsContainer.AddChild(this.fSprites[rect[i]]);
					this.fSprites[rect[i]].anchorX = 0f;
					this.fSprites[rect[i]].anchorY = 0f;
				}
				this.fSprites[rect[4]].alpha = 0.05f;
				break;
			default:
				break;
			}
		}

		public override void Move(Vector2 newPos)
		{
			data.SetValue<Vector2>(field.key, newPos);
			base.Move(newPos); // calls refresh
		}

		public override void Refresh()
		{
			base.Refresh();
			pos = data.GetValue<Vector2>(field.key);

			if (line >= 0)
			{
				base.MoveSprite(line, this.absPos);
				this.fSprites[line].scaleY = pos.magnitude;
				//this.fSprites[line].rotation = RWCustom.Custom.AimFromOneVectorToAnother(this.absPos, (parentNode as PositionedDevUINode).absPos); // but why
				this.fSprites[line].rotation = RWCustom.Custom.VecToDeg(-pos);
			}
			if (circle >= 0)
			{
				base.MoveSprite(circle, (parentNode as PositionedDevUINode)!.absPos);
				this.fSprites[circle].scale = pos.magnitude / 8f;
				this.fSprites[circle].alpha = 2f / pos.magnitude;
			}
			if (rect != null)
			{
				Vector2 leftbottom = Vector2.zero;
				Vector2 topright = Vector2.zero;
				// rectgrid abandoned

				leftbottom = (parentNode as PositionedDevUINode)!.absPos + leftbottom;
				topright = absPos + topright;
				Vector2 size = (topright - leftbottom);

				base.MoveSprite(rect[0], leftbottom);
				this.fSprites[rect[0]].scaleY = size.y;// + size.y.Sign();
				base.MoveSprite(rect[1], leftbottom);
				this.fSprites[rect[1]].scaleX = size.x;// + size.x.Sign();
				base.MoveSprite(rect[2], (topright));
				this.fSprites[rect[2]].scaleY = -size.y;// - size.y.Sign();
				base.MoveSprite(rect[3], (topright));
				this.fSprites[rect[3]].scaleX = -size.x;// - size.x.Sign();
				base.MoveSprite(rect[4], leftbottom);
				this.fSprites[rect[4]].scaleX = size.x;// + size.x.Sign();
				this.fSprites[rect[4]].scaleY = size.y;// + size.y.Sign();
			}
		}

		public override void SetColor(Color col)
		{
			base.SetColor(col);
			if (line >= 0)
			{
				this.fSprites[line].color = col;
			}
			if (circle >= 0)
			{
				this.fSprites[circle].color = col;
			}

			if (rect != null)
			{
				for (int i = 0; i < rect.Length; i++)
				{
					this.fSprites[rect[i]].color = col;
				}
			}
		}
	}

	public class ManagedIntHandle : Handle // All-in-one super handle 2
	{
		protected readonly IntVector2Field field;
		protected readonly ManagedData data;
		protected readonly IntVector2Field.IntVectorReprType reprType;
		protected readonly int pixel = -1;
		protected readonly int[]? rect;

		public ManagedIntHandle(
			IntVector2Field field,
			ManagedData managedData,
			ManagedRepresentation repr,
			IntVector2Field.IntVectorReprType reprType) : base(
				repr.owner,
				field.key,
				repr,
				managedData.GetValue<RWCustom.IntVector2>(field.key).ToVector2() * 20f)
		{
			this.field = field;
			this.data = managedData;
			this.reprType = reprType;
			switch (reprType)
			{
			case IntVector2Field.IntVectorReprType.line:
			case IntVector2Field.IntVectorReprType.tile:
			case IntVector2Field.IntVectorReprType.fourdir:
			case IntVector2Field.IntVectorReprType.eightdir:
				this.pixel = this.fSprites.Count;
				this.fSprites.Add(new FSprite("pixel", true));
				owner.placedObjectsContainer.AddChild(this.fSprites[pixel]);
				this.fSprites[pixel].MoveBehindOtherNode(this.fSprites[0]); // attention to detail

				if (reprType == IntVector2Field.IntVectorReprType.tile)
				{
					this.fSprites[pixel].alpha = 0.25f;
					this.fSprites[pixel].scale = 20f;
				}

				this.fSprites[pixel].anchorX = 0;
				this.fSprites[pixel].anchorY = 0;

				if (reprType is IntVector2Field.IntVectorReprType.fourdir or IntVector2Field.IntVectorReprType.eightdir)
				{
					this.fSprites[0].SetElementByName("Menu_Symbol_Arrow");
					this.fSprites[0].scale = 0.75f;
				}

				break;
			case IntVector2Field.IntVectorReprType.rect:
				this.rect = new int[5];
				for (int i = 0; i < 5; i++)
				{
					this.rect[i] = this.fSprites.Count;
					this.fSprites.Add(new FSprite("pixel", true));
					owner.placedObjectsContainer.AddChild(this.fSprites[rect[i]]);
					this.fSprites[rect[i]].anchorX = 0f;
					this.fSprites[rect[i]].anchorY = 0f;
				}
				this.fSprites[rect[4]].alpha = 0.05f;
				break;
			default:
				break;
			}
		}

		public override void Move(Vector2 newPos)
		{
			// absolute so we're aligned with room tiles
			if (reprType == IntVector2Field.IntVectorReprType.fourdir)
			{
				float dir = RWCustom.Custom.VecToDeg(newPos);
				dir -= ((dir + 45f + 360f) % 90f) - 45f;
				newPos = RWCustom.Custom.DegToVec(dir) * 20f;
			}
			else if (reprType == IntVector2Field.IntVectorReprType.eightdir)
			{
				float dir = RWCustom.Custom.VecToDeg(newPos);
				dir -= ((dir + 22.5f + 360f) % 45f) - 22.5f;
				newPos = RWCustom.Custom.DegToVec(dir);
				if (Mathf.Abs(newPos.x) > 0.5f) newPos.x = Mathf.Sign(newPos.x);
				if (Mathf.Abs(newPos.y) > 0.5f) newPos.y = Mathf.Sign(newPos.y);
				newPos *= 20f;
			}
			Vector2 parentPos = (this.parentNode as PositionedDevUINode)!.pos + this.owner.game.cameras[0].pos;
			Vector2 roompos = newPos + parentPos;
			RWCustom.IntVector2 ownIntPos = new RWCustom.IntVector2(Mathf.FloorToInt(roompos.x / 20f), Mathf.FloorToInt(roompos.y / 20f));
			RWCustom.IntVector2 parentIntPos = new RWCustom.IntVector2(Mathf.FloorToInt(parentPos.x / 20f), Mathf.FloorToInt(parentPos.y / 20f));
			// relativize again
			ownIntPos -= parentIntPos;
			newPos = ownIntPos.ToVector2() * 20f;

			//Vector2 roompos = newPos;
			//RWCustom.IntVector2 ownIntPos = new RWCustom.IntVector2(Mathf.FloorToInt(roompos.x / 20f), Mathf.FloorToInt(roompos.y / 20f));
			//newPos = ownIntPos.ToVector2() * 20f;

			data.SetValue<RWCustom.IntVector2>(field.key, ownIntPos);
			base.Move(newPos); // calls refresh
		}

		public override void Refresh()
		{
			base.Refresh();
			pos = data.GetValue<RWCustom.IntVector2>(field.key).ToVector2() * 20f;

			if (pixel >= 0)
			{
				switch (reprType)
				{

				case IntVector2Field.IntVectorReprType.tile:
					base.MoveSprite(pixel, new Vector2(Mathf.FloorToInt(absPos.x / 20f), Mathf.FloorToInt(absPos.y / 20f)) * 20f);
					break;
				case IntVector2Field.IntVectorReprType.line:
				case IntVector2Field.IntVectorReprType.fourdir:
				case IntVector2Field.IntVectorReprType.eightdir:
					if (reprType == IntVector2Field.IntVectorReprType.fourdir || reprType == IntVector2Field.IntVectorReprType.eightdir)
						this.fSprites[0].rotation = RWCustom.Custom.VecToDeg(pos);

					base.MoveSprite(pixel, this.absPos);
					this.fSprites[pixel].scaleY = pos.magnitude;
					this.fSprites[pixel].rotation = RWCustom.Custom.VecToDeg(-pos);
					break;
				}
			}

			if (rect != null)
			{
				Vector2 parentPos = (this.parentNode as PositionedDevUINode)!.pos + this.owner.game.cameras[0].pos;
				Vector2 roompos = pos + parentPos;
				Vector2 offset = -this.owner.game.cameras[0].pos;
				RWCustom.IntVector2 ownIntPos = new RWCustom.IntVector2(Mathf.FloorToInt(roompos.x / 20f), Mathf.FloorToInt(roompos.y / 20f));
				RWCustom.IntVector2 parentIntPos = new RWCustom.IntVector2(Mathf.FloorToInt(parentPos.x / 20f), Mathf.FloorToInt(parentPos.y / 20f));

				Vector2 leftbottom = offset + new Vector2(Mathf.Min(ownIntPos.x, parentIntPos.x) * 20f, Mathf.Min(ownIntPos.y, parentIntPos.y) * 20f);
				Vector2 topright = offset + new Vector2(Mathf.Max(ownIntPos.x, parentIntPos.x) * 20f + 20f, Mathf.Max(ownIntPos.y, parentIntPos.y) * 20f + 20f);
				// rectgrid revived

				Vector2 size = (topright - leftbottom);

				base.MoveSprite(rect[0], leftbottom);
				this.fSprites[rect[0]].scaleY = size.y;// + size.y.Sign();
				base.MoveSprite(rect[1], leftbottom);
				this.fSprites[rect[1]].scaleX = size.x;// + size.x.Sign();
				base.MoveSprite(rect[2], (topright));
				this.fSprites[rect[2]].scaleY = -size.y;// - size.y.Sign();
				base.MoveSprite(rect[3], (topright));
				this.fSprites[rect[3]].scaleX = -size.x;// - size.x.Sign();
				base.MoveSprite(rect[4], leftbottom);
				this.fSprites[rect[4]].scaleX = size.x;// + size.x.Sign();
				this.fSprites[rect[4]].scaleY = size.y;// + size.y.Sign();
			}
		}

		public override void SetColor(Color col)
		{
			base.SetColor(col);
			if (pixel >= 0)
			{
				this.fSprites[pixel].color = col;
			}

			if (rect != null)
			{
				for (int i = 0; i < rect.Length; i++)
				{
					this.fSprites[rect[i]].color = col;
				}
			}
		}
	}

	public class ManagedSlider : Slider
	{
		protected readonly ManagedFieldWithPanel field;
		protected readonly IInterpolablePanelField interpolable;
		protected readonly ManagedData data;

		public ManagedSlider(ManagedFieldWithPanel field,
					ManagedData data,
					DevUINode parent,
					float sizeOfDisplayname) : base(
						parent.owner,
						field.key,
						parent,
						Vector2.zero,
						sizeOfDisplayname > 0 ? field.displayName : "",
						false,
						sizeOfDisplayname)
		{
			this.field = field;
			this.interpolable = (field as IInterpolablePanelField)!;
			if (interpolable is null) throw new ArgumentException("Field must implement IInterpolablePanelField");
			this.data = data;

			DevUILabel numberLabel = (this.subNodes[1] as DevUILabel)!;

			numberLabel.pos.x = sizeOfDisplayname + 10f;
			numberLabel.size.x = field.SizeOfLargestDisplayValue();
			numberLabel.fSprites[0].scaleX = numberLabel.size.x;

			// hacky hack for nubpos
			this.titleWidth = sizeOfDisplayname + numberLabel.size.x - 16f;
		}

		public override void NubDragged(float nubPos)
		{
			interpolable!.NewFactor(this, data, nubPos);
			// this.managedControlPanel.managedRepresentation.Refresh(); // is this relevant ?
			this.Refresh();
		}

		public override void Refresh()
		{
			base.Refresh();
			float value = interpolable.FactorOf(this, data);
			base.NumberText = field.DisplayValueForNode(this, data);
			base.RefreshNubPos(value);
		}
	}

	public class ManagedButton : PositionedDevUINode, IDevUISignals
	{
		protected readonly Button button;
		protected readonly ManagedFieldWithPanel field;
		protected readonly IIterablePanelField iterable;
		protected readonly ManagedData data;
		public ManagedButton(
			ManagedFieldWithPanel field,
			ManagedData data,
			ManagedControlPanel panel,
			float sizeOfDisplayname) : base(
				panel.owner,
				field.key,
				panel,
				Vector2.zero)

		{
			this.field = field;
			this.iterable = (field as IIterablePanelField)!;
			if (iterable == null) throw new ArgumentException("Field must implement IIterablePanelField");
			this.data = data;
			this.subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0f, 0f), sizeOfDisplayname, field.displayName));
			this.subNodes.Add(this.button = new Button(owner, "Button", this, new Vector2(sizeOfDisplayname + 10f, 0f), field.SizeOfLargestDisplayValue(), field.DisplayValueForNode(this, data)));
		}

		public virtual void Signal(DevUISignalType type, DevUINode sender, string message) // from button
		{
			iterable.Next(this, data);
			this.Refresh();
		}

		public override void Refresh()
		{
			this.button.Text = field.DisplayValueForNode(this, data);
			base.Refresh();
		}
	}

	public class ManagedArrowSelector : IntegerControl
	{
		protected readonly ManagedFieldWithPanel field;
		protected readonly IIterablePanelField iterable;
		protected readonly ManagedData data;

		public ManagedArrowSelector(ManagedFieldWithPanel field,
			ManagedData managedData,
			ManagedControlPanel panel,
			float sizeOfDisplayname) : base(
				panel.owner,
				"ManagedArrowSelector",
				panel,
				Vector2.zero,
				field.displayName)
		{
			this.field = field;
			this.iterable = (field as IIterablePanelField)!;
			if (iterable == null) throw new ArgumentException("Field must implement IIterablePanelField");
			this.data = managedData;

			DevUILabel titleLabel = (this.subNodes[0] as DevUILabel)!;
			titleLabel.size.x = sizeOfDisplayname;
			titleLabel.fSprites[0].scaleX = sizeOfDisplayname;

			DevUILabel numberLabel = (this.subNodes[1] as DevUILabel)!;
			numberLabel.pos.x = sizeOfDisplayname + 30f;
			numberLabel.size.x = field.SizeOfLargestDisplayValue();
			numberLabel.fSprites[0].scaleX = numberLabel.size.x;

			ArrowButton arrowL = (this.subNodes[2] as ArrowButton)!;
			arrowL.pos.x = sizeOfDisplayname + 10f;

			ArrowButton arrowR = (this.subNodes[3] as ArrowButton)!;
			arrowR.pos.x = numberLabel.pos.x + numberLabel.size.x + 4f;
		}

		public override void Increment(int change)
		{
			if (change == 1)
			{
				iterable.Next(this, data);
			}
			else if (change == -1)
			{
				iterable.Prev(this, data);
			}

			this.Refresh();
		}

		public override void Refresh()
		{
			NumberLabelText = field.DisplayValueForNode(this, data);
			base.Refresh();
		}
	}

	public class ManagedStringControl : PositionedDevUINode
	{
		public static ManagedStringControl? activeStringControl = null;

		protected readonly ManagedFieldWithPanel field;
		protected readonly ManagedData data;
		protected bool clickedLastUpdate = false;

		public ManagedStringControl(
			ManagedFieldWithPanel field,
			ManagedData data,
			DevUINode panel,
			float sizeOfDisplayname)
				: base(
					  panel.owner,
					  "ManagedStringControl",
					  panel,
					  Vector2.zero)
		{
			SetupInputDetours();
			_text = null!;
			this.field = field;
			this.data = data;

			subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0, 0), sizeOfDisplayname, field.displayName));
			subNodes.Add(new DevUILabel(owner, "Text", this, new Vector2(60, 0), 136, ""));

			Text = field.DisplayValueForNode(this, data)!;

			DevUILabel textLabel = (this.subNodes[1] as DevUILabel)!;
			textLabel.pos.x = sizeOfDisplayname + 10f;
			textLabel.size.x = field.SizeOfLargestDisplayValue();
			textLabel.fSprites[0].scaleX = textLabel.size.x;
		}

		private string _text;
		protected virtual string Text
		{
			get
			{
				return _text;// subNodes[1].fLabels[0].text;
			}
			set
			{
				_text = value;
				subNodes[1].fLabels[0].text = value;
			}
		}

		public override void Refresh()
		{
			// No data refresh until the transaction is complete :/
			// TrySet happens on input and focus loss
			base.Refresh();
		}

		public override void Update()
		{
			if (owner.mouseClick && !clickedLastUpdate)
			{
				if ((subNodes[1] as RectangularDevUINode)!.MouseOver && activeStringControl != this)
				{
					// replace whatever instance/null that was focused
					 Text = field.DisplayValueForNode(this, data);
					activeStringControl = this;
					subNodes[1].fLabels[0].color = new Color(0.1f, 0.4f, 0.2f);
				}
				else if (activeStringControl == this)
				{
					// focus lost
					TrySetValue(Text, true);
					activeStringControl = null;
					subNodes[1].fLabels[0].color = Color.black;
				}

				clickedLastUpdate = true;
			}
			else if (!owner.mouseClick)
			{
				clickedLastUpdate = false;
			}

			if (activeStringControl == this)
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
						activeStringControl = null;
						subNodes[1].fLabels[0].color = Color.black;
					}
					else
					{
						Text += c;
						TrySetValue(Text, false);
					}
				}
			}
		}

		protected virtual void TrySetValue(string newValue, bool endTransaction)
		{
			try
			{
				field.ParseFromText(this, data, newValue);
				subNodes[1].fLabels[0].color = new Color(0.1f, 0.4f, 0.2f); // positive feedback
			}
			catch (Exception)
			{
				subNodes[1].fLabels[0].color = Color.red; // negative fedback
			}
			if (endTransaction)
			{
				Text = field.DisplayValueForNode(this, data)!;
				subNodes[1].fLabels[0].color = Color.black;
				Refresh();
			}
		}
	}

}
