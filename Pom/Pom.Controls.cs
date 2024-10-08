using System.Linq;
using DevInterface;
using UnityEngine;

namespace Pom;

public static partial class Pom
{
	// An undocumented mess. Have a look around, find what suits you, maybe implement your own.
	// in commit d2dad8768371565bb9b538263ac0b0ac595913b7 there was a slider-button used for text before text input became a thing
	// an arrows-text combo would also be amazing for enums and ints wink wink
	/// <summary>
	/// Managed handle controlling a single Vector2 value. Used for ALL vector2 representation types.
	/// </summary>
	public class ManagedVectorHandle : Handle // All-in-one super handle
	{
		/// <summary>
		/// Field definition
		/// </summary>
		protected readonly Vector2Field field;
		/// <summary>
		/// Data of the associated placedobject
		/// </summary>
		protected readonly ManagedData data;
		/// <summary>
		/// Chosen representation type
		/// </summary>
		protected readonly Vector2Field.VectorReprType reprType;
		/// <summary>
		/// Sprite index of the line (if it's below 0, the line does not exist)
		/// </summary>
		protected readonly int line = -1;
		/// <summary>
		/// Sprite index of the circle (if it's below 0, the circle does not exist)
		/// </summary>
		protected readonly int circle = -1;
		/// <summary>
		/// Sprite indices of rect (if it's null, the rect does not exist). Length is 5, ind. 0-3 are bounding lines, 4 is fill
		/// </summary>
		protected readonly int[]? rect;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="field">Field definition</param>
		/// <param name="managedData">Data of the associated placedobject</param>
		/// <param name="repr">Representation object</param>
		/// <param name="reprType">Chosen representation type</param>
		public ManagedVectorHandle(
			Vector2Field field,
			ManagedData managedData,
			ManagedRepresentation repr,
			Vector2Field.VectorReprType reprType) : base(
				repr.owner,
				field.key,
				repr,
				managedData.GetValue<Vector2>(field.key))
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
		/// <inheritdoc/>
		public override void Move(Vector2 newPos)
		{
			data.SetValue<Vector2>(field.key, newPos);
			base.Move(newPos); // calls refresh
		}
		/// <inheritdoc/>
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
		/// <inheritdoc/>
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
	/// <summary>
	/// A managed handle controlling an IntVector2 value. Used for ALL IntVector representation types.
	/// </summary>
	public class ManagedIntHandle : Handle // All-in-one super handle 2
	{
		/// <summary>
		/// Field definition.
		/// </summary>
		protected readonly IntVector2Field field;
		/// <summary>
		/// Data of the associated placedobject
		/// </summary>
		protected readonly ManagedData data;
		/// <summary>
		/// Chosen representation type.
		/// </summary>
		protected readonly IntVector2Field.IntVectorReprType reprType;
		/// <summary>
		/// Sprite index of the pixel (?????) (if below 0, pixel sprite doesn't exist)
		/// </summary>
		protected readonly int pixel = -1; //todo: check wtf it actually draws
		/// <summary>
		/// Sprite indices of the bounding rect (if null, doesn't exist). Length is 5, indices 0-3 are lines, 4 is fill
		/// </summary>
		protected readonly int[]? rect;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="field">Field definition</param>
		/// <param name="managedData">Data of the associated placedobject</param>
		/// <param name="repr">Representation object</param>
		/// <param name="reprType">Chosen representation type</param>
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
		/// <inheritdoc/>
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
		/// <inheritdoc/>
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
		/// <inheritdoc/>
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
	/// <summary>
	/// A multipurpose slider that can control data defined by any ManagedField that implements 
	/// <see cref="global::Pom.Pom.IInterpolablePanelField"/>
	/// </summary>
	public class ManagedSlider : Slider
	{
		/// <summary>
		/// Definition of the field this slider is related to
		/// </summary>
		protected readonly ManagedFieldWithPanel field;
		/// <summary>
		/// <see cref="field"/>, but cast to the interface type
		/// </summary>
		protected readonly IInterpolablePanelField interpolable;
		/// <summary>
		/// Data of the associated placedobject
		/// </summary>
		protected readonly ManagedData data;
		/// <param name="field">Field definition</param>
		/// <param name="data">Data of the associated placedobject</param>
		/// <param name="parent">Parent DevUI node</param>
		/// <param name="sizeOfDisplayname">Width of name tag, can be 0</param>
		public ManagedSlider(
			ManagedFieldWithPanel field,
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
		/// <inheritdoc/>
		public override void NubDragged(float nubPos)
		{
			interpolable!.NewFactor(this, data, nubPos);
			// this.managedControlPanel.managedRepresentation.Refresh(); // is this relevant ?
			this.Refresh();
		}
		/// <inheritdoc/>
		public override void Refresh()
		{
			base.Refresh();
			float value = interpolable.FactorOf(this, data);
			base.NumberText = field.DisplayValueForNode(this, data);
			base.RefreshNubPos(value);
		}
	}
	/// <summary>
	/// A multipurpose button that can control data defined by any ManagedField that implements 
	/// <see cref="global::Pom.Pom.IIterablePanelField"/>
	/// </summary>
	public class ManagedButton : PositionedDevUINode, IDevUISignals
	{
		/// <summary>
		/// Contained button
		/// </summary>
		protected readonly Button button;
		/// <summary>
		/// Field definition
		/// </summary>
		protected readonly ManagedFieldWithPanel field;
		/// <summary>
		/// <see cref="field"/> but cast to interface type
		/// </summary>
		protected readonly IIterablePanelField iterable;
		/// <summary>
		/// Data of the associated placedobject
		/// </summary>
		protected readonly ManagedData data;
		/// <param name="field">Field definition</param>
		/// <param name="data">Data of the associated placedobject</param>
		/// <param name="panel">Containing panel</param>
		/// <param name="sizeOfDisplayname">Size of name tag, can be 0</param>
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
		/// <inheritdoc/>
		public virtual void Signal(DevUISignalType type, DevUINode sender, string message) // from button
		{
			iterable.Next(this, data);
			this.Refresh();
		}
		/// <inheritdoc/>
		public override void Refresh()
		{
			this.button.Text = field.DisplayValueForNode(this, data);
			base.Refresh();
		}
	}
	/// <summary>
	/// Multipurpose arrow selector that can control data defined by any ManagedField that implements 
	/// <see cref="global::Pom.Pom.IIterablePanelField"/>
	/// </summary>
	public class ManagedArrowSelector : IntegerControl
	{
		/// <summary>
		/// Field definition
		/// </summary>
		protected readonly ManagedFieldWithPanel field;
		/// <summary>
		/// <see cref="field"/> but cast to interface type
		/// </summary>
		protected readonly IIterablePanelField iterable;
		/// <summary>
		/// Data of the associated placedobject
		/// </summary>
		protected readonly ManagedData data;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="field">Field definition</param>
		/// <param name="managedData">Data of the associated placedobject</param>
		/// <param name="panel">Containing panel</param>
		/// <param name="sizeOfDisplayname">Size of name tag, can be 0</param>
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
		/// <inheritdoc/>
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
		/// <inheritdoc/>
		public override void Refresh()
		{
			NumberLabelText = field.DisplayValueForNode(this, data);
			base.Refresh();
		}
	}
	/// <summary>
	/// String input field. Can be used by things like 
	/// <see cref="global::Pom.Pom.StringField"/> or <see cref="global::Pom.Pom.ColorField"/>
	/// </summary>
	public class ManagedStringControl : PositionedDevUINode, IDevUISignals
	{
		StringControl stringControl;
		/// <summary>
		/// Currently focused string control. Can be any devui node, including not from POM assembly. As long as it's not null, POM should prevent anything else from taking keyboard input
		/// </summary>
		public static DevUINode? activeStringControl = null;
		/// <summary>
		/// Field definition for this string control
		/// </summary>
		protected readonly ManagedFieldWithPanel field;
		/// <summary>
		/// Data of the associated placedobject
		/// </summary>
		protected readonly ManagedData data;

		/// <param name="field">Field definition</param>
		/// <param name="data">Data of the associated placedobject</param>
		/// <param name="panel">Containing panel</param>
		/// <param name="sizeOfDisplayname">Size of name tag, can be 0</param>
		public ManagedStringControl(
			ManagedFieldWithPanel field,
			ManagedData data,
			DevUINode panel,
			float sizeOfDisplayname) : base(
				panel.owner,
				"ManagedStringControl",
				panel,
				Vector2.zero)
		{
			this.field = field;
			this.data = data;

			stringControl = new StringControl(owner, "Text", this, new Vector2(60, 0), 136, field.DisplayValueForNode(this, data)!, StringControl.TextIsAny);
			subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0, 0), sizeOfDisplayname, field.displayName));
			subNodes.Add(stringControl);

			stringControl.pos.x = sizeOfDisplayname + 10f;
			stringControl.size.x = field.SizeOfLargestDisplayValue();
			stringControl.fSprites[0].scaleX = stringControl.size.x;
		}
		/// <summary>
		/// Text value of the instance. 
		/// Changing this only changes the label, does not call <see cref="global::Pom.Pom.ManagedData.SetValue"/>.
		/// </summary>
		protected virtual string Text
		{
			get
			{
				return stringControl.Text;
			}
			set
			{
				stringControl.Text = value;
			}
		}
		/// <summary>
		/// Attempts to parse field value from given text and set it into manageddata. Recolors control depending on the result.
		/// </summary>
		/// <param name="newValue">Current text</param>
		protected virtual void TrySetValue(string newValue, bool endTransaction)
		{
			try
			{
				field.ParseFromText(this, data, newValue);
				stringControl.fLabels[0].color = new Color(0.1f, 0.4f, 0.2f); // positive feedback
			}
			catch (Exception ex)
			{
				LogWarning($"Failed to parse field from text: {ex}");
				stringControl.fLabels[0].color = Color.red; // negative fedback
			}
			if (endTransaction)
			{
				Text = field.DisplayValueForNode(this, data)!;
				stringControl.fLabels[0].color = Color.black;
				Refresh();
			}
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (sender == stringControl)
			{
				TrySetValue(stringControl.Text, type == StringControl.StringFinish);
			}
		}
	}

	public class StringControl : DevUILabel
	{
		public StringControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text, IsTextValid del) : base(owner, IDstring, parentNode, pos, width, text)
		{
			isTextValid = del;
			actualValue = text;
			Text = text;
			Refresh();
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
			// No data refresh until the transaction is complete :/
			// TrySet happens on input and focus loss
			base.Refresh();
		}

		public override void Update()
		{
			if (owner.mouseClick && !clickedLastUpdate)
			{
				if (MouseOver && ManagedStringControl.activeStringControl != this)
				{
					// replace whatever instance/null that was focused
					Text = actualValue;
					ManagedStringControl.activeStringControl = this;
					fLabels[0].color = new Color(0.1f, 0.4f, 0.2f);
				}
				else if (ManagedStringControl.activeStringControl == this)
				{
					// focus lost
					TrySetValue(Text, true);
					ManagedStringControl.activeStringControl = null;
					fLabels[0].color = Color.black;
				}

				clickedLastUpdate = true;
			}
			else if (!owner.mouseClick)
			{
				clickedLastUpdate = false;
			}

			if (ManagedStringControl.activeStringControl == this)
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
						ManagedStringControl.activeStringControl = null;
						fLabels[0].color = Color.black;
					}
					else
					{
						Text += c;
						TrySetValue(Text, false);
					}
				}
			}
		}

		public delegate bool IsTextValid(string value);

		public IsTextValid isTextValid;


		protected virtual void TrySetValue(string newValue, bool endTransaction)
		{
			if (isTextValid(newValue))
			{
				actualValue = newValue;
				fLabels[0].color = new Color(0.1f, 0.4f, 0.2f);
				this.SendSignal(StringEdit, this, "");
			}
			else
			{
				fLabels[0].color = Color.red;
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
		public static bool TextIsFloat(string value) => float.TryParse(value, out _);

		/// <summary>
		/// returns result of int.TryParse
		/// </summary>
		public static bool TextIsInt(string value) => (int.TryParse(value, out int i) && i.ToString() == value);


		public static bool TextIsColor(string value)
		{
			try { Color color = RWCustom.Custom.hexToColor(value); return RWCustom.Custom.colorToHex(color) == value; }
			catch { return false; }
		}

		/// <summary>
		/// returns true when text is ExtEnum
		/// </summary>
		public static bool TextIsExtEnum<T>(string value) where T : ExtEnum<T> => ExtEnumBase.TryParse(typeof(T), value, false, out _);

		/// <summary>
		/// always returns true
		/// </summary>
		public static bool TextIsAny(string value) => true;

		/// <summary>
		/// returns true if text could be a filename
		/// </summary>
		public static bool TextIsValidFilename(string value) => value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

		/// <summary>
		/// signal value sent by <see cref="StringControl"/> when value is changed without editing
		/// </summary>
		public static readonly DevUISignalType StringEdit = new DevUISignalType("StringEdit", true);
		/// <summary>
		/// signal value sent by <see cref="StringControl"/> when editing mode is finished
		/// </summary>
		public static readonly DevUISignalType StringFinish = new DevUISignalType("StringFinish", true);
	}

	/// <summary>
	/// A button that brings up a panel of all the selectable options
	/// <see cref="global::Pom.Pom.IListablePanelField"/>
	/// </summary>
	public class ManagedPanelButton : PositionedDevUINode, IDevUISignals
	{
		/// <summary>
		/// Contained button
		/// </summary>
		protected readonly Button button;
		/// <summary>
		/// Field definition
		/// </summary>
		protected readonly ManagedFieldWithPanel field;
		/// <summary>
		/// <see cref="field"/> but cast to interface type
		/// </summary>
		protected readonly IListablePanelField listable;
		/// <summary>
		/// Data of the associated placedobject
		/// </summary>
		protected readonly ManagedData data;
		/// <param name="field">Field definition</param>
		/// <param name="data">Data of the associated placedobject</param>
		/// <param name="panel">Containing panel</param>
		/// <param name="sizeOfDisplayname">Size of name tag, can be 0</param>
		public ManagedPanelButton(
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
			this.listable = (field as IListablePanelField)!;
			if (listable == null) throw new ArgumentException("Field must implement IListablePanelField");
			this.data = data;
			this.subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0f, 0f), sizeOfDisplayname, field.displayName));
			this.subNodes.Add(this.button = new Button(owner, "Button", this, new Vector2(sizeOfDisplayname + 10f, 0f), field.SizeOfLargestDisplayValue(), field.DisplayValueForNode(this, data)));
		}
		/// <inheritdoc/>
		public virtual void Signal(DevUISignalType type, DevUINode sender, string message) // from button
		{
			if (itemSelectPanel != null)
			{
				if (IsSubButtonID(sender.IDstring, out var item))
				{
					field.ParseFromText(this, data, item);
				}

				subNodes.Remove(itemSelectPanel);
				itemSelectPanel.ClearSprites();
				itemSelectPanel = null;
			}
			else
			{
				Vector2 setPos = panelPos - absPos;
				itemSelectPanel = new ItemSelectPanel(owner, this, setPos, IDstring + "_Panel", field.key, panelSize, panelButtonWidth, panelColumns);
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
			if (itemSelectPanel != null && IDstring.StartsWith(itemSelectPanel.idstring + "Button99289_"))
			{
				subButtonID = IDstring.Remove(0, (itemSelectPanel.idstring + "Button99289_").Length);
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public override void Refresh()
		{
			this.button.Text = field.DisplayValueForNode(this, data);
			base.Refresh();
		}

		/// <inheritdoc/>
		public string[] GetItems(int page)
		{
			return listable.GetValues(Math.Max(listable.LowestItem(), page * ItemsPerPage), Math.Min(listable.HighestItem(), (page + 1) * ItemsPerPage));
		}


		public ItemSelectPanel? itemSelectPanel;

		public Vector2 panelPos = new Vector2(200f, 15f);

		public Vector2 panelSize = new Vector2(305f, 420f);

		public float panelButtonWidth = 145f;

		public int ItemsPerPage => (int)((panelSize.y - 60f) / 20f * panelColumns);

		public int panelColumns = 2;
	}

	public class ItemSelectPanel : Panel, IDevUISignals
	{
		public ItemSelectPanel(
			DevUI owner,
			ManagedPanelButton parentNode,
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
			managedParent = parentNode;
			this.idstring = idstring;
			this.buttonWidth = buttonWidth;
			this.columns = columns;

			currentOffset = 0;
			perpage = (int)((this.size.y - 60f) / 20f * columns);
			PopulateItems(currentOffset);
		}
		public void PopulateItems(int offset)
		{
			string[] items = managedParent.GetItems(offset);
			if (items.Length == 0) return;
			currentOffset = offset;
			foreach (DevUINode devUINode in subNodes)
			{
				devUINode.ClearSprites();
			}

			subNodes.Clear();
			var intVector = new RWCustom.IntVector2(0, 0);

			foreach (string item in items)
			{

				var currentOption = new Button(owner, idstring + "Button99289_" + item, this, new Vector2(5f + intVector.x * (buttonWidth + 5f), size.y - 25f - 20f * intVector.y), buttonWidth, item);
				string currentItem = item;
				subNodes.Add(currentOption);
				intVector.y++;
				if (intVector.y >= (int)Mathf.Floor(perpage / columns))
				{
					intVector.x++;
					intVector.y = 0;
				}
			}

			float pageButtonWidth = (size.x - 15f) / 2f;

			subNodes.Add(new Button(owner, idstring + BACKBUTTON, this, new Vector2(5f, size.y - 25f - 20f * (perpage / columns + 1f)), pageButtonWidth, "Previous"));
			subNodes.Add(new Button(owner, idstring + NEXTBUTTON, this, new Vector2(size.x - 5f - pageButtonWidth, size.y - 25f - 20f * (perpage / columns + 1f)), pageButtonWidth, "Next"));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			//can't modify subnodes during Signal, as it is called during a loop through all subnodes
			if (sender.IDstring == idstring + BACKBUTTON)
			{ goPrev = true; }

			else if (sender.IDstring == idstring + NEXTBUTTON)
			{ goNext = true; }

			else { managedParent.Signal(type, sender, message); }
		}

		public override void Update()
		{
			if (goNext)
			{ PopulateItems(currentOffset + 1); goNext = false; }

			if (goPrev)
			{ PopulateItems(currentOffset - 1); goPrev = false; }

			base.Update();
		}

		const string BACKBUTTON = "BackPage99289..?/~";
		const string NEXTBUTTON = "NextPage99289..?/~";


		private ManagedPanelButton managedParent;

		private bool goNext = false;

		private bool goPrev = false;

		public string idstring;

		private float buttonWidth;

		private int columns;

		private int perpage;

		private int currentOffset;
	}
	public class TextInputSlider : Slider, IDevUISignals
	{
		public float ActualValue
		{
			get => _actualValue;
			set
			{
				_actualValue = value;
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
			subNodes[1] = new StringControl(owner, IDstring, this, new Vector2(titleWidth + 10f, 0f), inheritButton ? stringWidth + 26f : stringWidth, _actualValue.ToString(), StringControl.TextIsFloat);

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

			if (valueRounding >= 0)
			{ _actualValue = (float)Math.Round(_actualValue, valueRounding); }

			string str = "";
			if (_actualValue == defaultValue && inheritButton) str = "<D>";
			str += " ";
			if (displayRounding >= 0) str += Math.Round(_actualValue, displayRounding).ToString();
			else str += _actualValue.ToString();

			NumberText = str;
			(subNodes[1] as StringControl)!.ActualValue = _actualValue.ToString();

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
				_actualValue = float.Parse((subNodes[1] as StringControl)!.ActualValue);
				Refresh();
			}

			this.SendSignal(SliderUpdate, this, "");
		}

		public static readonly DevUISignalType SliderUpdate = new DevUISignalType("SliderUpdate", true);
	}

	/// <summary>
	/// A multipurpose button that can control data defined by any ManagedField that implements 
	/// <see cref="global::Pom.Pom.IIterablePanelField"/>
	/// </summary>
	public class ManagedRGBSelectButton : PositionedDevUINode, IDevUISignals
	{
		/// <summary>
		/// Contained button
		/// </summary>
		protected readonly RGBSelectButton button;
		/// <summary>
		/// Field definition
		/// </summary>
		protected readonly ColorField field;
		/// <summary>
		/// Data of the associated placedobject
		/// </summary>
		protected readonly ManagedData data;
		/// <param name="field">Field definition</param>
		/// <param name="data">Data of the associated placedobject</param>
		/// <param name="panel">Containing panel</param>
		/// <param name="sizeOfDisplayname">Size of name tag, can be 0</param>
		public ManagedRGBSelectButton(
			ColorField field,
			ManagedData data,
			ManagedControlPanel panel,
			float sizeOfDisplayname) : base(
				panel.owner,
				field.key,
				panel,
				Vector2.zero)

		{
			this.field = field;
			this.data = data;
			this.subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0f, 0f), sizeOfDisplayname, field.displayName));
			this.subNodes.Add(this.button = new RGBSelectButton(owner, "Button", this, new Vector2(sizeOfDisplayname + 10f, 0f), field.SizeOfLargestDisplayValue(), field.DisplayValueForNode(this, data) ?? "", (Color)field.DefaultValue, "Select a color"));
		}
		/// <inheritdoc/>
		public virtual void Signal(DevUISignalType type, DevUINode sender, string message) // from button
		{
			data.SetValue<Color>(field.key, button.actualValue);
			this.Refresh();
		}
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
				RGBSelectPanel = new RGBSelectPanel(owner, this, panelPos - pos, IDstring + "_Panel", panelName, actualValue);
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

		public Vector2 panelPos = new Vector2(420f, 280f);
	}

	public class RGBSelectPanel : Panel, IDevUISignals
	{
		private TextInputSlider[] RGBSliders;
		private TextInputSlider[] HSLSliders;
		private StringControl Hex;
		private ColorBox colorBox;
		private Color _actualValue = new(0f, 0f, 0f);
		public Color ActualValue
		{
			get => _actualValue;
			set
			{
				_actualValue = value;
				Refresh();
			}
		}

		public RGBSelectPanel(DevUI owner, DevUINode parentNode, Vector2 pos, string idstring, string title, Color color)
			: base(owner, idstring, parentNode, pos, new Vector2(305f, 200f), title)
		{
			RGBSliders = new TextInputSlider[3];
			HSLSliders = new TextInputSlider[3];
			//^ all set in PlaceNodes
			this.idstring = idstring;
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
				ActualValue = new Color(RGBSliders[0].ActualValue, RGBSliders[1].ActualValue, RGBSliders[2].ActualValue);
			}
			else if (HSLSliders.Contains(sender))
			{
				ActualValue = RWCustom.Custom.HSL2RGB(HSLSliders[0].ActualValue, HSLSliders[1].ActualValue, HSLSliders[2].ActualValue);
			}

			else if (sender == Hex)
			{
				ActualValue = RWCustom.Custom.hexToColor(Hex.ActualValue);
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
			UpdateRGBSliders();
			UpdateHSLSliders();
			UpdateHex();
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
		public class ColorButton : DevUILabel
		{
			private bool down = false;
			protected Color _actualValue;
			public ColorButton(DevUI owner, DevUINode parentNode, Vector2 pos, string idstring, float width, float height, Color color) : base(owner, idstring, parentNode, pos, width, "")
			{
				fSprites[0].scaleY = height;
				_actualValue = color;
				fSprites[0].color = color;
			}

			public Color Color
			{
				get => _actualValue;
				set
				{
					_actualValue = value;
					Refresh();
				}
			}

			public Color InvertedColor => new Color(1f - _actualValue.r, 1f - _actualValue.g, 1f - _actualValue.b);

			public override void Update()
			{
				base.Update();
				if (owner != null && owner.mouseClick && MouseOver)
				{
					this. down = true;
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
				//fSprites[0].color = !down ? Color : InvertedColor;
			}
		}

		public class ColorBox : MouseOverSwitchColorLabel
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
		public string idstring;

	}
}
