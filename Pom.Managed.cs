using DevInterface;
using System.Text.RegularExpressions;
using UnityEngine;

using System.Reflection;

namespace Pom;

public static partial class Pom
{

	/// <summary>
	/// Main class for managed object types.
	/// Make-calls CAN return null, causing the object to not be created, or have no data, or use the default handle representation.
	/// This class can be used to simply handle the room/devtools hooks.
	/// Call <see cref="RegisterManagedObject(ManagedObjectType)"/> to register your manager
	/// </summary>
	public class ManagedObjectType
	{
		protected PlacedObject.Type? placedType;
		protected readonly string name;
		protected readonly Type? objectType;
		protected readonly Type? dataType;
		protected readonly Type? reprType;
		protected readonly bool singleInstance;
		/// <summary>
		/// Creates a ManagedObjectType responsible for creating your placedobject instance, data and repr
		/// </summary>
		/// <param name="name">The enum-name this manager responds for. Do NOT use EnumExt_MyEnum.MyObject.ToString() because on mod-loading enumextender might not have run yet and your enums aren't extended.</param>
		/// <param name="objectType">The Type of your UpdateableAndDeletable object. Must have a constructor like (Room room, PlacedObject pObj) or (PlacedObject pObj, Room room), (PlacedObject pObj) or (Room room).</param>
		/// <param name="dataType">The Type of your PlacedObject.Data. Must have a constructor like (PlacedObject pObj).</param>
		/// <param name="reprType">The Type of your PlacedObjectRepresentation. Must have a constructor like (DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) or (PlacedObject.Type placedType, ObjectsPage objPage, PlacedObject pObj).</param>
		/// <param name="singleInstance">Wether only one of this object should be created per room. Corruption-object that scans for other placedobjects style.</param>

		public ManagedObjectType(
			string name,
			string category,
			Type? objectType,
			Type? dataType,
			Type? reprType,
			bool singleInstance = false)
		{
			if (name is null) throw new ArgumentNullException(nameof(name));
			if (category is null) throw new ArgumentNullException(nameof(category));
			this.placedType = new(name, true); //no longer deferred
			__objectCategories.Add(name, new(category, true));
			//__moddedTypes.Add(name);
			this.name = name;
			this.objectType = objectType;
			this.dataType = dataType;
			this.reprType = reprType;
			this.singleInstance = singleInstance;
		}

		/// <summary>
		/// The <see cref="PlacedObject.Type"/> this is the manager for.
		/// Only call this after rainworld.start call otherwise EnumExtender might not be available.
		/// Store a reference to your <see cref="ManagedObjectType"/> instead of the enum type.
		/// </summary>
		public virtual PlacedObject.Type GetObjectType()
		{
			return placedType == default ? placedType = DeclareOrGetEnum(name) : placedType;
		}

		/// <summary>
		/// Called from Room.Loaded hook
		/// </summary>
		public virtual UpdatableAndDeletable? MakeObject(PlacedObject placedObject, Room room)
		{
			if (objectType == null) return null;

			if (singleInstance) // Only one per room
			{
				UpdatableAndDeletable instance = null!;
				foreach (var item in room.updateList)
				{
					if (item.GetType().IsAssignableFrom(objectType))
					{
						instance = item;
						break;
					}
				}
				if (instance != null) return null;
			}

			try { return (UpdatableAndDeletable)Activator.CreateInstance(objectType, new object[] { room, placedObject }); }
			catch (MissingMethodException)
			{
				try { return (UpdatableAndDeletable)Activator.CreateInstance(objectType, new object[] { placedObject, room }); }
				catch (MissingMethodException)
				{
					try { return (UpdatableAndDeletable)Activator.CreateInstance(objectType, new object[] { placedObject }); }
					catch (MissingMethodException)
					{
						try { return (UpdatableAndDeletable)Activator.CreateInstance(objectType, new object[] { room }); } // Objects that scan room for data or no data;
						catch (MissingMethodException) { throw new ArgumentException("ManagedObjectType.MakeObject : objectType " + objectType.Name + " must have a constructor like (Room room, PlacedObject pObj) or (PlacedObject pObj, Room room) or (Room room)"); }
					}
				}
			}
		}

		/// <summary>
		/// Called from PlacedObject.GenerateEmptyData hook
		/// </summary>
		public virtual PlacedObject.Data? MakeEmptyData(PlacedObject pObj)
		{
			if (dataType == null) return null;

			try { return (PlacedObject.Data)Activator.CreateInstance(dataType, new object[] { pObj }); }
			catch (MissingMethodException)
			{
				try { return (PlacedObject.Data)Activator.CreateInstance(dataType, new object[] { pObj, PlacedObject.LightFixtureData.Type.RedLight }); } // Redlights man
				catch (MissingMethodException) { throw new ArgumentException("ManagedObjectType.MakeEmptyData : dataType " + dataType.Name + " must have a constructor like (PlacedObject pObj)"); }
			}
		}

		/// <summary>
		/// Called from ObjectsPage.CreateObjRep hook
		/// </summary>
		public virtual PlacedObjectRepresentation? MakeRepresentation(PlacedObject pObj, ObjectsPage objPage)
		{
			if (reprType is null || placedType is null) return null;

			try { return (PlacedObjectRepresentation)Activator.CreateInstance(reprType, new object[] { objPage.owner, placedType.ToString() + "_Rep", objPage, pObj, placedType.ToString() }); }
			catch (MissingMethodException)
			{
				try { return (PlacedObjectRepresentation)Activator.CreateInstance(reprType, new object[] { objPage.owner, placedType.ToString() + "_Rep", objPage, pObj, placedType.ToString(), false }); } // Resizeables man
				catch (MissingMethodException)
				{
					try { return (PlacedObjectRepresentation)Activator.CreateInstance(reprType, new object[] { pObj.type, objPage, pObj }); } // Our own silly types
					catch (MissingMethodException) { throw new ArgumentException("ManagedObjectType.MakeRepresentation : reprType " + reprType.Name + " must have a constructor like (DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) or (PlacedObject.Type placedType, ObjectsPage objPage, PlacedObject pObj)"); }
				}
			}
		}
	}

	/// <summary>
	/// Class for managing a wraped <see cref="UpdatableAndDeletable"/> object
	/// Uses the fully managed data and representation types <see cref="ManagedData"/> and <see cref="ManagedRepresentation"/>
	/// </summary>
	public class FullyManagedObjectType : ManagedObjectType
	{
		protected readonly ManagedField[] managedFields;

		public FullyManagedObjectType(
			string? name,
			string? category,
			Type objectType,
			ManagedField[] managedFields,
			bool singleInstance = false) : base(
				name ?? objectType.Name,
				category ?? "POM",
				objectType,
				null,
				null,
				singleInstance)
		{
			this.managedFields = managedFields;
		}

		public override PlacedObject.Data MakeEmptyData(PlacedObject pObj)
		{
			return new ManagedData(pObj, managedFields);
		}

		public override PlacedObjectRepresentation MakeRepresentation(PlacedObject pObj, ObjectsPage objPage)
		{
			return new ManagedRepresentation(GetObjectType(), objPage, pObj);
		}
	}

	/// <summary>
	/// A field to handle serialization and generate UI for your data, for use with <see cref="ManagedData"/>.
	/// A field is merely a recipe/interface, the actual data is stored in the <see cref="ManagedData"/> data object for each pObj.
	/// You can use a field as an <see cref="Attribute"/> anotating data fields in your class that inherits <see cref="ManagedData"/> so they stay in sync.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public abstract class ManagedField : Attribute
	{
		public readonly string key;
		protected object defaultValue; // Removed reaonly, now one can be clever with defaults that apply to the next object being placed and such.
		public virtual object DefaultValue => defaultValue; // I SUSPECT one day someone will run into a situation with enums where the default value doesn't exist at initialization. Enumextend moment. Lets be nice to that poor soul.

		protected ManagedField(
			string key,
			object defaultValue)
		{
			this.key = key;
			this.defaultValue = defaultValue;
		}

		/// <summary>
		/// Serialization method called from <see cref="ManagedData.ToString"/>
		/// </summary>
		public virtual string ToString(object value) => value.ToString();

		/// <summary>
		/// Deserialization method called from <see cref="ManagedData.FromString"/>. Don't forget to sanitize your data.
		/// </summary>
		public abstract object FromString(string str);

		/// <summary>
		/// Wether this field spawns a control panel node or not. Inherit <see cref="ManagedFieldWithPanel"/> for actually creating them.
		/// </summary>
		public virtual bool NeedsControlPanel { get => this is ManagedFieldWithPanel; }

		/// <summary>
		/// Create an aditional DevUINode for manipulating this field. Inherit a PositionedDevUINode if you need to create several sub-nodes.
		/// </summary>
		public virtual DevUINode? MakeAditionalNodes(
			ManagedData managedData,
			ManagedRepresentation managedRepresentation)
		{
			return null;
		}

		// Stop inheriting crap :/
		public sealed override bool IsDefaultAttribute() { return base.IsDefaultAttribute(); }
		public sealed override bool Match(object obj) { return base.Match(obj); }
		public sealed override object TypeId => base.TypeId;
	}

	/// <summary>
	/// A <see cref="ManagedField"/> that can generate a control-panel control, for use with <see cref="ManagedRepresentation"/>
	/// </summary>
	public abstract class ManagedFieldWithPanel : ManagedField
	{
		protected readonly ControlType control;
		public readonly string displayName;

		protected ManagedFieldWithPanel(
			string key,
			object defaultValue,
			ControlType control = ControlType.none,
			string? displayName = null) : base(
				key,
				defaultValue)
		{
			this.control = control;
			this.displayName = displayName ?? key;
		}

		public enum ControlType
		{
			none,
			slider,
			arrows,
			button,
			text
		}

		/// <summary>
		/// Used internally for control panel display. 
		/// Consumed by <see cref="ManagedRepresentation.MakeControls"/> to expand the panel and space controls.
		/// </summary>
		public virtual Vector2 SizeOfPanelUiMinusName()
		{
			return SizeOfPanelNode() + new Vector2(SizeOfLargestDisplayValue(), 0f);
		}

		/// <summary>
		/// Used internally for control panel display. Consumed by <see cref="SizeOfPanelUiMinusName"/> and final UI nodes.
		/// </summary>
		public abstract float SizeOfLargestDisplayValue();

		/// <summary>
		/// Approx size of the UI minus displayname and valuedisplay width. Consumed by <see cref="SizeOfPanelUiMinusName"/>.
		/// </summary>
		protected virtual Vector2 SizeOfPanelNode()
		{
			switch (control)
			{
			case ControlType.slider:
				return new Vector2(116f, 20f);

			case ControlType.arrows:
				return new Vector2(52f, 20f);

			case ControlType.text:
			case ControlType.button:
				return new Vector2(12f, 20f);

			default:
				break;
			}
			return new Vector2(0f, 20f);
		}

		/// <summary>
		/// Used internally for control panel display.
		/// Called from <see cref="ManagedRepresentation.MakeControls"/>
		/// </summary>
		public virtual float SizeOfDisplayname()
		{
			return HUD.DialogBox.meanCharWidth * (displayName.Length + 2);
		}

		/// <summary>
		/// Used internally for building the control panel display.
		/// Called from <see cref="ManagedRepresentation.MakeControls"/>
		/// </summary>
		public virtual PositionedDevUINode? MakeControlPanelNode(
			ManagedData managedData,
			ManagedControlPanel panel,
			float sizeOfDisplayname)
		{
			switch (control)
			{
			case ControlType.slider:
				return new ManagedSlider(this, managedData, panel, sizeOfDisplayname);
			case ControlType.arrows:
				return new ManagedArrowSelector(this, managedData, panel, sizeOfDisplayname);
			case ControlType.button:
				return new ManagedButton(this, managedData, panel, sizeOfDisplayname);
			case ControlType.text:
				return new ManagedStringControl(this, managedData, panel, sizeOfDisplayname);
			}
			return null;
		}

		/// <summary>
		/// Used internally for controls in the panel to display the value of this field as text.
		/// </summary>
		public virtual string? DisplayValueForNode(
			PositionedDevUINode node,
			ManagedData data)
		{
			// field tostring, but fields can format on their own
			return ToString(data.GetValue<object>(key) ?? string.Empty);
		}

		/// <summary>
		/// Used internally for text input parsing by <see cref="ManagedStringControl"/>.
		/// Should raise an <see cref="ArgumentException"/> if the value is invalid or can't be parsed (used for visual feedback on text input)
		/// </summary>
		public virtual void ParseFromText(
			PositionedDevUINode node,
			ManagedData data,
			string newValue)
		{
			data.SetValue(key, this.FromString(newValue));
		}
	}


	/// <summary>
	/// Managed data type, handles managed fields passed through the constuctor and through Attributes.
	/// </summary>
	public class ManagedData : PlacedObject.Data
	{
		public readonly ManagedField[] fields;
		protected readonly Dictionary<string, FieldInfo> fieldInfosByKey;
		protected readonly Dictionary<string, ManagedField> fieldsByKey;
		protected readonly Dictionary<string, object> valuesByKey;

		/// <summary>
		/// Attribute for tying a field to a <see cref="ManagedField"/> that cannot be properly initialized as Attribute such as <see cref="Vector2Field"/> and <see cref="EnumField"/>.
		/// </summary>
		[AttributeUsage(AttributeTargets.Field)]
		protected class BackedByField : Attribute
		{
			public string key;

			public BackedByField(string key)
			{
				this.key = key;
			}
		}

		/// <summary>
		/// Instantiates the managed data object for use with a placed object in the roomSettings.
		/// You shouldn't instantiate this on your own, it'll be called by the framework.
		/// </summary>
		/// <param name="owner">the <see cref="PlacedObject"/> this data belongs to</param>
		/// <param name="paramFields">the <see cref="ManagedField"/>s for this data. Upon initialization it'll also scan for any annotated fields.</param>
		public ManagedData(
			PlacedObject owner,
			ManagedField[]? paramFields) : base(owner)
		{
			paramFields = paramFields ?? new ManagedField[0];
			this.fields = paramFields;
			this.fieldsByKey = new Dictionary<string, ManagedField>();
			this.valuesByKey = new Dictionary<string, object>();
			this.fieldInfosByKey = new Dictionary<string, FieldInfo>();

			panelPos = new Vector2(100, 50);

			// Scan for annotated fields
			List<ManagedField> attrFields = new List<ManagedField>();
			foreach (FieldInfo fieldInfo in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			{
				object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(ManagedField), true);

				foreach (object attr in customAttributes) // There should be only one or zero anyways
				{
					ManagedField fieldAttr = (ManagedField)attr;
					attrFields.Add(fieldAttr);
					fieldInfosByKey[fieldAttr.key] = fieldInfo;
					fieldInfo.SetValue(this, fieldAttr.DefaultValue);
				}
			}
			if (attrFields.Count > 0) // any annotated fields
			{
				attrFields.Sort((f1, f2) => string.Compare(f1.key, f2.key)); // type.GetFields() does NOT guarantee order
				this.fields = paramFields.Concat(attrFields).ToArray();
			}

			// go through all fields, passed as parameter or annotated
			this.NeedsControlPanel = false;
			foreach (var field in this.fields)
			{
				if (fieldsByKey.ContainsKey(field.key)) throw new ArgumentException("Fields with duplicated names : " + field.key);
				fieldsByKey[field.key] = field;
				valuesByKey[field.key] = field.DefaultValue;
				if (field.NeedsControlPanel) this.NeedsControlPanel = true;
			}

			// link backed fields
			foreach (FieldInfo fieldInfo in this.GetType().GetFields())
			{
				object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(BackedByField), true);

				foreach (BackedByField fieldAttr in customAttributes) // There should be only one anyways ??? As long as they have different keys everything will be fiiiiine
				{
					if (!fieldsByKey.ContainsKey(fieldAttr.key)) throw new ArgumentException("No such field for BackedByField : " + fieldAttr.key + ". Are you sure you created this field ?");
					if (fieldInfosByKey.ContainsKey(fieldAttr.key)) throw new ArgumentException("BackedByField for field already backing another field : " + fieldAttr.key);
					fieldInfosByKey[fieldAttr.key] = fieldInfo;
					fieldInfo.SetValue(this, valuesByKey[fieldAttr.key]);
				}
			}
		}

		public Vector2 panelPos;
		public virtual bool NeedsControlPanel { get; protected set; }
		/// <summary>
		/// For classes that inherit this to know where their data begins. Create something similar for your class if you intend it to be inherited further ;)
		/// </summary>
		protected int FieldsWhenSerialized => fields.Length + (NeedsControlPanel ? 2 : 0);

		/// <summary>
		/// Retrieves the value stored for the field represented by this key.
		/// </summary>
		public virtual T? GetValue<T>(string fieldName)
			where T : notnull
		{
			if (fieldInfosByKey.TryGetValue(fieldName, out FieldInfo field))
				return (T)field.GetValue(this);
			else if (valuesByKey.TryGetValue(fieldName, out object res))
				return (T)res;
			else return default;
		}

		/// <summary>
		/// Stores a new value for the field represented by this key. Used mostly by the managed UI. Changes are only saved when the Save button is clicked on the devtools ui
		/// </summary>
		public virtual void SetValue<T>(
			string fieldName,
			T value)
			where T : notnull
		{
			if (fieldInfosByKey.TryGetValue(fieldName, out FieldInfo field))
				field.SetValue(this, value);
			else
				valuesByKey[fieldName] = (object)value;
		}

		/// <summary>
		/// Deserialization function called when the placedobject for this data is loaded
		/// </summary>
		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			int datastart = 0;
			if (NeedsControlPanel)
			{
				this.panelPos = new Vector2(float.Parse(array[0]), float.Parse(array[1]));
				datastart = 2;
			}

			for (int i = 0; i < fields.Length; i++)
			{
				if (array.Length == datastart + i)
				{
					Debug.LogError("Error: Not enough fields for managed data type for "
						+ owner.type.ToString()
						+ "\nMaybe there's a version missmatch between the settings and the running version of the mod.");
					break;
				}
				try
				{
					object val = fields[i].FromString(array[datastart + i]);
					SetValue(fields[i].key, val);
				}
				catch (Exception)
				{
					Debug.LogError("Error parsing field "
						+ fields[i].key
						+ " from managed data type for "
						+ owner.type.ToString()
						+ "\nMaybe there's a version missmatch between the settings and the running version of the mod.");
				}
			}
		}

		/// <summary>
		/// Serialization function called when the placedobject for this data is saved with devtools.
		/// </summary>
		public override string ToString()
		{
			return (NeedsControlPanel ? (panelPos.x.ToString() + "~" + panelPos.y.ToString() + "~") : "") + string.Join("~", Array.ConvertAll(fields, f => f.ToString(GetValue<object>(f.key) ?? string.Empty)));
		}
	}

	/// <summary>
	/// Class that manages the PlacedObjectRepresentation for a <see cref="ManagedData"/>, 
	/// creating controls for any <see cref="ManagedField"/> that needs them,
	/// or panel UI for <see cref="ManagedFieldWithPanel"/>.
	/// </summary>
	public class ManagedRepresentation : PlacedObjectRepresentation
	{
		protected readonly PlacedObject.Type placedType;
		public readonly Dictionary<string, DevUINode> managedNodes; // Unused for now, but seems convenient for specialization
		protected ManagedControlPanel? panel; // Unused for now, but seems convenient for specialization
		public ManagedRepresentation(
			PlacedObject.Type placedType,
			ObjectsPage objPage,
			PlacedObject pObj) : base(
				objPage.owner,
				placedType.ToString() + "_Rep",
				objPage,
				pObj,
				placedType.ToString())
		{
			this.placedType = placedType;
			this.pObj = pObj;
			this.managedNodes = new Dictionary<string, DevUINode>();
			MakeControls();
		}

		protected virtual void MakeControls()
		{
			ManagedData data = (ManagedData)pObj.data;
			if (data.NeedsControlPanel)
			{
				ManagedControlPanel panel = new ManagedControlPanel(this.owner, "ManagedControlPanel", this, data.panelPos, Vector2.zero, pObj.type.ToString());
				this.panel = panel;
				this.subNodes.Add(panel);
				Vector2 uiSize = new Vector2(0f, 0f);
				Vector2 uiPos = new Vector2(3f, 3f);
				float largestDisplayname = 0f;
				for (int i = 0; i < data.fields.Length; i++) // up down
				{
					if (data.fields[i] is ManagedFieldWithPanel field && field.NeedsControlPanel)
					{
						largestDisplayname = Mathf.Max(largestDisplayname, field.SizeOfDisplayname());
					}
				}

				for (int i = data.fields.Length - 1; i >= 0; i--) // down up
				{
					if (data.fields[i] is ManagedFieldWithPanel field && field.NeedsControlPanel)
					{
						PositionedDevUINode? node = field.MakeControlPanelNode(data, panel, largestDisplayname);
						if (node is null)
						{
							throw new InvalidDataException();

						}
						panel.managedNodes[field.key] = node;
						panel.managedFields[field.key] = field;
						panel.subNodes.Add(node);
						node.pos = uiPos;
						uiSize.x = Mathf.Max(uiSize.x, field.SizeOfPanelUiMinusName().x);
						uiSize.y += field.SizeOfPanelUiMinusName().y;
						uiPos.y += field.SizeOfPanelUiMinusName().y;
					}
				}
				panel.size = uiSize + new Vector2(3 + largestDisplayname, 1);
			}

			for (int i = 0; i < data.fields.Length; i++)
			{
				ManagedField field = data.fields[i];
				DevUINode? node = field.MakeAditionalNodes(data, this);
				if (node != null)
				{
					this.subNodes.Add(node);
					this.managedNodes[field.key] = node;
				}
			}
		}
	}

	/// <summary>
	/// The panel spawned by <see cref="ManagedRepresentation"/> if any of its <see cref="ManagedField"/>s requires panel UI.
	/// Doesn't do much on its own besides keeping a white line that connects the panel and the placedobject representation.
	/// </summary>
	public class ManagedControlPanel : Panel
	{
		protected readonly ManagedRepresentation managedRepresentation;
		public Dictionary<string, DevUINode> managedNodes; // Added from ManagedRepresentation. Unused for now, but seems convenient for specialization
		public Dictionary<string, ManagedFieldWithPanel> managedFields; // Added from ManagedRepresentation. Unused for now, but seems convenient for specialization
		protected readonly int lineSprt;

		public ManagedControlPanel(DevUI owner, string IDstring, ManagedRepresentation parentNode, Vector2 pos, Vector2 size, string title) : base(owner, IDstring, parentNode, pos, size, title)
		{
			managedRepresentation = parentNode;
			managedNodes = new Dictionary<string, DevUINode>();
			managedFields = new Dictionary<string, ManagedFieldWithPanel>();

			this.fSprites.Add(new FSprite("pixel", true));
			owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprt = this.fSprites.Count - 1]);
			this.fSprites[lineSprt].anchorY = 0f;
		}
		public override void Refresh()
		{
			base.Refresh();
			Vector2 bottomLeft = collapsed ? nonCollapsedAbsPos + new Vector2(0f, size.y) : this.absPos;
			base.MoveSprite(lineSprt, bottomLeft);
			this.fSprites[lineSprt].scaleY = collapsed ? (this.pos + new Vector2(0f, size.y)).magnitude : this.pos.magnitude;
			this.fSprites[lineSprt].rotation = RWCustom.Custom.AimFromOneVectorToAnother(bottomLeft, (parentNode as PositionedDevUINode)!.absPos);
			(this.managedRepresentation.pObj.data as ManagedData)!.panelPos = this.pos; // mfw no "data with panel" intermediate class
		}
	}
}
