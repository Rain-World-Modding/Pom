using DevInterface;
using RWCustom;
using UnityEngine;

namespace Pom;
// TODO: Solve a way to create or destroy Vector2ListHandles for the Nodes with Dev Tools Buttons
// Written by Cactus
public static partial class Pom
{
	/// <summary>
	/// A <see cref="ManagedField"/> that stores an array of <see cref="Vector2"/>s that can be modified in size.
	/// </summary>
	public class Vector2ListField : ManagedFieldWithPanel, IIterablePanelField, IInterpolablePanelField, IListablePanelField
	{
		/// <summary>
		/// Chosen representation type
		/// </summary>
		public Vector2ListRepresentationType RepresentationType;
		/// <summary>
		/// Number of vector2 nodes
		/// </summary>
		public int NodeCount;
		public bool IncludeParent;
		public int Maximum;

		/// <summary>
		/// Creates a <see cref="Vector2ListField"/> and assigns the proper values that are used in the handle
		/// </summary>
		/// <param name="key">The key of the field that should be used in <see cref="ManagedData.GetValue{T}"/></param>
		/// <param name="maximum">The max number of <see cref="Vector2"/>s that the field stores</param>
		/// <param name="includeParent">Sets if the field should use the parent as the first node</param>
		/// <param name="representationType">The type of the representation that should be created.</param>
// TODO: Change Vector2[] nodes to be set for 4 nodes or a list, or even make it accept a min and max node count?
// TODO: Remove includeParent?
		public Vector2ListField(
			string key,
			int maximum,
			bool includeParent,
			Vector2ListRepresentationType representationType = Vector2ListRepresentationType.Polygon
			) : base(
				key,
				ProcessNodes())
		{
			NodeCount = 4;
			RepresentationType = representationType;
			IncludeParent = includeParent;
			Maximum = maximum;
		}

		/// <inheritdoc/>
		public override string ToString(object value)
		{
			Vector2[] vectors = (Vector2[])value;
			return string.Join("^", vectors.Select(v => $"{v.x};{v.y}").ToArray());
		}
		/// <inheritdoc/>
		public override object FromString(string str)
		{
			List<Vector2> positions = new List<Vector2>();
			foreach (string substring in str.Split('^'))
			{
				string[] split = substring.Split(';');
				positions.Add(new Vector2(float.Parse(split[0]), float.Parse(split[1])));
			}

			return positions.ToArray();
		}

		// Make a square for the base node set
		private static object ProcessNodes()
		{
			Vector2[] result =
			{
				new (40f, 40f),
				new (40f, -40f),
				new (-40f, -40f),
				new (-40f, 40f)
			};
			
			return result;
		}
		/// <inheritdoc/>
		public override DevUINode MakeAditionalNodes(ManagedData managedData, ManagedRepresentation managedRepresentation)
		{
			return new Vector2ListWrapper(this, managedData, managedRepresentation);
		}
		/// <summary>
		/// How the handle should look like
		/// </summary>
		public enum Vector2ListRepresentationType
		{
			/// <summary>
			/// Handle is an open-ended chain
			/// </summary>
			Chain,
			/// <summary>
			/// Handle is a polygon (start and end connected)
			/// </summary>
			Polygon
		}
		/// <summary>
		/// Special handle for vector2listfield
		/// </summary>
// TODO: Solve a way to create or destroy Vector2ListHandles for the Nodes with Dev Tools Buttons
		public class Vector2ListWrapper : PositionedDevUINode
		{
			private Vector2ListInt iteratorControl;
			/// <summary>
			/// Associated field
			/// </summary>
			public Vector2ListField Field;
			/// <summary>
			/// Associated placedobject's data
			/// </summary>
			public ManagedData Data;
			/// <summary>
			/// First node
			/// </summary>
			public PositionedDevUINode First;
			/// <summary>
			/// List of line subnode indices
			/// </summary>
			public List<int> Lines = new List<int>();

			/// <param name="field">Field definition</param>
			/// <param name="data">Associated placedobject's data</param>
			/// <param name="representation">Associated placedobject's representation</param>
			public Vector2ListWrapper(
				Vector2ListField field,
				ManagedData data,
				ManagedRepresentation representation) : base(
					representation.owner,
					field.key,
					representation,
					(data.GetValue<Vector2[]>(field.key) ?? new Vector2[] { default })[0])
			{
				Field = field;
				
				Data = data;
				bool includeParent = Field.IncludeParent;
				if (includeParent)
				{
					First = (parentNode as PositionedDevUINode)!;
				}
				else
				{
					First = new Handle(owner, field.key + "_0", this, Data.GetValue<Vector2[]>(field.key)![0]);
					subNodes.Add(First);
				}

				for (int i = 1; i < field.NodeCount; i++)
				{
					int currentLine = fSprites.Count;
					PositionedDevUINode next = new Handle(owner, field.key + "_" + i, this, Data.GetValue<Vector2[]>(field.key)![i]);
					subNodes.Add(next);
					fSprites.Add(new FSprite("pixel"));
					owner.placedObjectsContainer.AddChild(fSprites[currentLine]);
					fSprites[currentLine].anchorY = 0;
					Lines.Add(currentLine);
				}

				if (Field.RepresentationType == Vector2ListRepresentationType.Polygon)
				{
					int currLine = fSprites.Count;
					fSprites.Add(new FSprite("pixel"));
					owner.placedObjectsContainer.AddChild(fSprites[currLine]);
					fSprites[currLine].anchorY = 0;
					Lines.Add(currLine);
				}

				iteratorControl = new Vector2ListInt(Field, Data, new ManagedControlPanel());
			}
			/// <inheritdoc/>
			// TODO: Figure out how to set control node to (0, 0) or other value if IncludeParent is false
			public override void Move(Vector2 newPos)
			{
				First.Move(newPos);
				Vector2[] vArr = Data.GetValue<Vector2[]>(Field.key)!;
				vArr[0] = Field.IncludeParent ? new Vector2(0, 0) : newPos;
				Data.SetValue(Field.key, vArr);
				base.Move(newPos);
			}
			/// <inheritdoc/>
			public override void Refresh()
			{
				base.Refresh();
				PositionedDevUINode start = First;
				int offset = Field.IncludeParent ? 0 : 1;
				for (int i = offset; i < subNodes.Count; i++)
				{
					if (!(subNodes[i] is PositionedDevUINode end))
					{
						throw new NullReferenceException("end node cannot be null!!");
					}

					int lineIndex = Lines[i - offset];
					MoveSprite(lineIndex, start.absPos);
					FSprite lineSprite = fSprites[lineIndex];
					lineSprite.scaleY = (end.absPos - start.absPos).magnitude;
					lineSprite.rotation = Custom.VecToDeg(end.absPos - start.absPos);
					start = end;
				}

				if (Field.RepresentationType == Vector2ListRepresentationType.Polygon)
				{
					int lastIndex = Lines[Lines.Count - 1];
					MoveSprite(lastIndex, start.absPos);
					FSprite lineSprite = fSprites[lastIndex];
					lineSprite.scaleY = (start.absPos - First.absPos).magnitude;
					lineSprite.rotation = Custom.VecToDeg(First.absPos - start.absPos);
				}
			}
		}
		public class Vector2ListInt : IntegerControl
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
		public Vector2ListInt(ManagedFieldWithPanel field,
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
		// Note to self: This makes an array of vector2s out of every two floats in the given array!
		// Don't even need the other make nodes i think... since you can always add or subtract a node.
		private static Vector2[] MakeVectors(float[] floats)
		{
			Vector2[] res = new Vector2[floats.Length / 2];
			for (int i = 0; i < res.Length; i++)
			{
				res[i] = new(floats[i * 2], floats[i * 2 + 1]);
			}
			return res;
		}

		public override float SizeOfLargestDisplayValue()
		{
			return HUD.DialogBox.meanCharWidth * ((Mathf.Max(Mathf.Abs(0), Mathf.Abs(Maximum))).ToString().Length + 2);
		}

		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Next(
			PositionedDevUINode node,
			ManagedData data)
		{
			int val = data.GetValue<int>(key) + 1;
			if (val > Maximum) val = 0;
			data.SetValue<int>(key, val);
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Prev(
			PositionedDevUINode node,
			ManagedData data)
		{
			int val = data.GetValue<int>(key) - 1;
			if (val < 0) val = Maximum;
			data.SetValue<int>(key, val);
		}

		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual float FactorOf(
			PositionedDevUINode node,
			ManagedData data)
		{
			return (Maximum - 0 == 0) ? 0f : (data.GetValue<int>(key) - 0) / (float)(Maximum - 0);
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual void NewFactor(
			PositionedDevUINode node,
			ManagedData data,
			float factor)
		{
			data.SetValue<int>(key, Mathf.RoundToInt(0 + factor * (Maximum - 0)));
		}

		public string[] GetValues(int start, int end)
		{
			List<string> values = new();
			for (int i = start; i < end; i++)
			{
				values.Add(i.ToString());
			}
			return values.ToArray();
		}

		public int HighestItem() => Maximum + 1;

		public int LowestItem() => 0;
	}
}
