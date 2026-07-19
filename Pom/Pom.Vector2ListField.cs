using DevInterface;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pom;

// Written by Cactus
public static partial class Pom
{
	
	/// <summary>
	/// A <see cref="ManagedField"/> that stores an array of <see cref="Vector2"/>s that can be modified in size.
	/// </summary>
	public class Vector2ListField : ManagedFieldWithPanel, IInterpolablePanelField, IListablePanelField, IIterablePanelField
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
		public int Minimum;

		/// <summary>
		/// Creates a <see cref="Vector2ListField"/> and assigns the proper values that are used in the handle
		/// </summary>
		/// <param name="key">The key of the field that should be used in <see cref="ManagedData.GetValue{T}"/></param>
		/// <param name="maximum">The max number of <see cref="Vector2"/>s that the field stores</param>
		/// <param name="includeParent">Sets if the field should use the parent as the first node</param>
		/// <param name="representationType">The type of the representation that should be created.</param>
// TODO: Remove includeParent?
// TODO: Fix first node's position influencing other node positions on save
		public Vector2ListField(
			string key,
			int maximum,
			int minimum,
			Vector2ListRepresentationType representationType = Vector2ListRepresentationType.Polygon
			) : base(
				key,
				ProcessNodes(3))
		{
			NodeCount = 3;
			RepresentationType = representationType;
			IncludeParent = true;
			Maximum = maximum;
			Minimum = minimum;
		}

		// The position of the int handle is added after the ToString and ForString methods of the list...
		/// <inheritdoc/>
		public override string ToString(object value)
		{
			// First toString position is from the position of the int handle
			// Separates the vectors by "^", Separates the vector components by ";" 
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
		
		public override PositionedDevUINode? MakeControlPanelNode(
			ManagedData managedData,
			ManagedControlPanel panel,
			float sizeOfDisplayname)
		{
			return new ManagedArrowSelector(this, managedData, panel, 72f);
		}
		
		public override string? DisplayValueForNode(
			PositionedDevUINode node,
			ManagedData data)
		{
			// field tostring, but fields can format on their own
			return NodeCount.ToString();
		}
		
		
		
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Next(
			PositionedDevUINode node,
			ManagedData data)
		{
			if (NodeCount < Maximum)
			{
				// Save data from previous array and resize
				int oldLength = NodeCount;
				NodeCount++;

				
				Vector2[] copy = new Vector2[oldLength + 1];
				Vector2 newPos = new Vector2(Random.value * 500f * (Random.value - 0.5f), Random.value * 500f * (Random.value - 0.5f));
				copy[copy.Length - 1] = newPos;
				
				Array.Copy(data.GetValue<Vector2[]>(key), copy, oldLength);
				
				data.SetValue(key, copy);
				
				// Adds the handle to remove the node
				List<DevInterface.DevUINode> uiList = node.parentNode.parentNode.subNodes;

				for (int i = 0; i < uiList.Count; i++)
				{
					if (uiList[i].IDstring.Equals(this.key)) // Gets this current field
					{
						this.NodeCount = data.GetValue<Vector2[]>(this.key).Length;
						
						for (int j = 0; j < uiList[i].subNodes.Count; j++)
						{
							uiList[i].subNodes[j].ClearSprites();
						}
						for (int j = 0; j < uiList[i].fSprites.Count; j++)
						{
							uiList[i].fSprites[j].container.RemoveChild(uiList[i].fSprites[j]);
						}
						uiList[i] = new Vector2ListHandle(this, data, (uiList[i] as Vector2ListHandle).rep);
						break;
					}
				}
			}
		}
		
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Prev(
			PositionedDevUINode node,
			ManagedData data)
		{
			if (NodeCount > Minimum && NodeCount > 1)
			{
				// Save data from previous array and resize
				int oldLength = NodeCount;
				NodeCount--;
				
				Vector2[] copy = new Vector2[oldLength - 1];
				Array.Copy(data.GetValue<Vector2[]>(key), copy, oldLength - 2);
				
				data.SetValue(key, copy);
				
				// Reloads the handle to remove the node
				List<DevInterface.DevUINode> uiList = node.parentNode.parentNode.subNodes;

				for (int i = 0; i < uiList.Count; i++)
				{
					if (uiList[i].IDstring.Equals(this.key)) // Gets this current field
					{
						this.NodeCount = data.GetValue<Vector2[]>(this.key).Length;
						//
						for (int j = 0; j < uiList[i].subNodes.Count; j++)
						{
							uiList[i].subNodes[j].ClearSprites();
						}
						for (int j = 0; j < uiList[i].fSprites.Count; j++)
						{
							uiList[i].fSprites[j].container.RemoveChild(uiList[i].fSprites[j]);
						}
						uiList[i] = new Vector2ListHandle(this, data, (uiList[i] as Vector2ListHandle).rep);
						break;
					}
				}
			}
		}
		
		private static object ProcessNodes(int nodeCount)
		{
			return new Vector2[nodeCount];
		}
		
		/// <inheritdoc/>
		public override DevUINode MakeAditionalNodes(ManagedData managedData, ManagedRepresentation managedRepresentation)
		{
			this.NodeCount = managedData.GetValue<Vector2[]>(this.key).Length;
			return new Vector2ListHandle(this, managedData, managedRepresentation);
		}
		/// <summary>
		/// How the handle should look like
		/// </summary>
		public enum Vector2ListRepresentationType
		{
			/// <summary>
			/// Handle is a polygon (start and end connected)
			/// </summary>
			Polygon
		}
		/// <summary>
		/// Special handle for vector2listfield
		/// </summary>
// TODO: Solve a way to create or destroy Vector2ListHandles for the Nodes with Dev Tools Buttons
		public class Vector2ListHandle : PositionedDevUINode
		{
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

			internal ManagedRepresentation rep;
			/// <summary>
			/// List of line subnode indices
			/// </summary>
			public List<int> Lines = new List<int>();

			/// <param name="field">Field definition</param>
			/// <param name="data">Associated placedobject's data</param>
			/// <param name="representation">Associated placedobject's representation</param>
			public Vector2ListHandle(
				Vector2ListField field,
				ManagedData data,
				ManagedRepresentation representation) : base(
					representation.owner,
					field.key,
					representation,
					(data.GetValue<Vector2[]>(field.key) ?? new Vector2[] { default })[0])
			{
				Field = field;
				rep = representation;
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
				
			}
			/// <inheritdoc/>
			// TODO: Figure out how to set control node to (0, 0) or other value if IncludeParent is false
			public override void Move(Vector2 newPos)
			{
				First.Move(newPos);
				Vector2[] vArr = Data.GetValue<Vector2[]>(Field.key)!;
				// Sets initial value to 0,0 if include parent is false
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
