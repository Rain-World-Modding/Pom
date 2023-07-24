using DevInterface;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using RWCustom;
using static Pom.Pom;

namespace Pom;
#pragma warning disable 1591

public static class Examples
{
	internal class OneOfAll : ManagedData
	{
		[IntegerField("int", 0, 10, 0)]
		internal int Int;
		[FloatField("float", 0f, 10f, 0f)]
		internal float Float;
		[StringField("string", "AAAA")]
		internal string String = "AAAA";
		[Vector2Field("vector", 10f, 30f)]
		internal Vector2 Vec;
		[IntVector2Field("intvector", 3, 3, IntVector2Field.IntVectorReprType.tile)]
		internal IntVector2 IntVec;
		[Vector2ArrayField("vectorarray", 5, false, Vector2ArrayField.Vector2ArrayRepresentationType.Chain, 10f, 10f, 20f, 20f, 20f, -20f, -20f, -20f, -20f, 20f)]
		internal Vector2[] VecArray = new Vector2[0];
		[EnumField<BepInEx.Logging.LogLevel>("enum", BepInEx.Logging.LogLevel.Warning)]
		BepInEx.Logging.LogLevel Enum;
		[ExtEnumField<AbstractCreature.AbstractObjectType>("extenum", nameof(AbstractCreature.AbstractObjectType.AttachedBee), new[] { nameof(AbstractCreature.AbstractObjectType.AttachedBee), nameof(AbstractCreature.AbstractObjectType.BubbleGrass) })]
		AbstractCreature.AbstractObjectType ExtEnum = AbstractCreature.AbstractObjectType.AttachedBee;

		public OneOfAll(PlacedObject owner) : base(owner, null)
		{
		}
	}
	internal static void __RegisterExamples()
	{
		//__sortCategorySettings.Add(new("POM examples", true), CategorySortKind.Alphabetical);
		//__sortCategorySettings.Add(new("RegionKit", true), CategorySortKind.Alphabetical);
		RegisterEmptyObjectType<OneOfAll, ManagedRepresentation>("OneOfAll", "POM examples");
		// This is one possible approach to register an object, almost zero boilerplate code, but accessing these fields requires passing a key as seen in SillyObject class
		// Registers a type with a loooooot of fields
		List<ManagedField> fields = new List<ManagedField>
		{
			new FloatField("f1", 0f, 1f, 0.2f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Float Slider"),
			new FloatField("f2", 0f, 1f, 0.5f, 0.1f, ManagedFieldWithPanel.ControlType.button, "Float Button"),
			new FloatField("f3", 0f, 1f, 0.8f, 0.1f, ManagedFieldWithPanel.ControlType.arrows, "Float Arrows"),
			new FloatField("f4", 0f, 1f, 0.8f, 0.1f, ManagedFieldWithPanel.ControlType.text, "Float Text"),

			new BooleanField("b1", false, ManagedFieldWithPanel.ControlType.slider, "Bool Slider"),
			new BooleanField("b2", true, ManagedFieldWithPanel.ControlType.button, "Bool Button"),
			new BooleanField("b3", false, ManagedFieldWithPanel.ControlType.arrows, "Bool Arrows"),
			new BooleanField("b4", true, ManagedFieldWithPanel.ControlType.text, "Bool Text"),

			new ExtEnumField<PlacedObject.Type>("e1", PlacedObject.Type.None, new PlacedObject.Type[] { PlacedObject.Type.BlueToken, PlacedObject.Type.GoldToken }, ManagedFieldWithPanel.ControlType.slider, "Enum Slider"),
			new ExtEnumField<PlacedObject.Type>("e2", PlacedObject.Type.Mushroom, null, ManagedFieldWithPanel.ControlType.button, "Enum Button"),
			new ExtEnumField<PlacedObject.Type>("e3", PlacedObject.Type.SuperStructureFuses, null, ManagedFieldWithPanel.ControlType.arrows, "Enum Arrows"),
			new ExtEnumField<PlacedObject.Type>("e4", PlacedObject.Type.GhostSpot, null, ManagedFieldWithPanel.ControlType.text, "Enum Text"),

			new IntegerField("i1", 0, 10, 1, ManagedFieldWithPanel.ControlType.slider, "Integer Slider"),
			new IntegerField("i2", 0, 10, 2, ManagedFieldWithPanel.ControlType.button, "Integer Button"),
			new IntegerField("i3", 0, 10, 3, ManagedFieldWithPanel.ControlType.arrows, "Integer Arrows"),
			new IntegerField("i4", 0, 10, 3, ManagedFieldWithPanel.ControlType.text, "Integer Text"),

			new StringField("str1", "your text here", "String"),

			new Vector2Field("vf1", Vector2.one, Vector2Field.VectorReprType.line),
			new Vector2Field("vf2", Vector2.one, Vector2Field.VectorReprType.circle),
			new Vector2Field("vf3", Vector2.one, Vector2Field.VectorReprType.rect),

			new IntVector2Field("ivf1", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.line),
			new IntVector2Field("ivf2", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.tile),
			new IntVector2Field("ivf3", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.fourdir),
			new IntVector2Field("ivf4", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.eightdir),
			new IntVector2Field("ivf5", new RWCustom.IntVector2(1, 1), IntVector2Field.IntVectorReprType.rect)
		};

		// Data serialization and UI are taken care of by the manageddata and managedrepresentation types
		// And that's about it, now sillyobject will receive a placedobject with manageddata and that data will have all these fields
		// The enum is registered for you based on the name of the type you're passing in, but you can pass your own name as an optional parameter.
		RegisterFullyManagedObjectType(fields.ToArray(), typeof(SillyObject), "SillyObject", "POM examples");
		// Trust me I just spared you from writing some 300 lines of code with this.


		// A different approach for registering objects, more classes involved but acessing your data is much nicer.
		// Registers a self implemented Managed Object Type
		// It handles spawning its object, data and representation 
		RegisterManagedObject(new CuriousObjectType());
		// Could also be achieved with RegisterManagedObject(new ManagedObjectType("CuriousObject", typeof(CuriousObjectType.CuriousObject), typeof(CuriousObjectType.CuriousData), typeof(CuriousObjectType.CuriousRepresentation)));
		// but at the expense of some extra reflection calls

		// A type with no object, no data, no repr, just for marking places. PlacedObject.type "CuriousObjectLocation" is registered for you.
		ManagedObjectType curiousObjectLocation = new ManagedObjectType("CuriousObjectLocation", "POM examples", null, null, null);
		RegisterManagedObject(curiousObjectLocation);
		// Could also be done with RegisterEmptyObjectType("CuriousObjectLocation", null, null);
		//Pom.__objectCategories.Add(EnumExt_ManagedPlacedObjects.CuriousObject.value, new("POM examples", true));
		Pom.__objectCategories.Add(EnumExt_ManagedPlacedObjects.CuriousObjectLocation.value, new("POM examples", true));
	}

	// Juuuuust an object, yet, we can place it. Data and UI are generated automatically
	internal class SillyObject : UpdatableAndDeletable
	{
		private readonly PlacedObject placedObject;

		public SillyObject(PlacedObject pObj, Room room)
		{
			this.room = room;
			this.placedObject = pObj;
			//Mod.__.Log("SillyObject create");
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.clock % 100 == 0)
				LogMessage("SillyObject vf1.x is " + ((ManagedData)placedObject.data).GetValue<Vector2>("vf1").x); // This is how you access those fields you created when using ManagedData directly.
		}
	}


	// Some other objects, this time we're registering type, object, data and representation on our own
	public static class EnumExt_ManagedPlacedObjects
	{
		public static PlacedObject.Type CuriousObject = new(nameof(CuriousObject), true);
		public static PlacedObject.Type CuriousObjectLocation = new(nameof(CuriousObjectLocation), true);
	}

	// A very curious object, part managed part manual
	// Overriding the base class here was optional, you could have instantiated it passing all the types, but doing it like this saves some reflection calls.
	internal class CuriousObjectType : ManagedObjectType
	{
		// Ignore the stuff in the baseclass and write your own if you want to
		public CuriousObjectType() : base("CuriousObject", "POM examples", null, typeof(CuriousData), typeof(CuriousRepresentation)) // this could have been (PlacedObjects.CuriousObject, typeof(CuriousObject), typeof(...)...)
		{
		}

		// Override at your own risk ? the default behaviour works just fine if you passed in a name to the constructor, but maybe you know what you're doign
		//public override PlacedObject.Type GetObjectType()
		//{
		//    return EnumExt_ManagedPlacedObjects.CuriousObject;
		//}

		public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
		{
			return new CuriousObject(placedObject, room);
		}

		// Maybe you need different parameters for these ? You do you...
		//public override PlacedObject.Data MakeEmptyData(PlacedObject pObj)
		//{
		//    return new CuriousData(pObj);
		//}

		//public override PlacedObjectRepresentation MakeRepresentation(PlacedObject pObj, ObjectsPage objPage)
		//{
		//    return new CuriousRepresentation(GetObjectType(), objPage, pObj);
		//}

		// Our curious and useful object
		class CuriousObject : UpdatableAndDeletable, IDrawable
		{
			private readonly PlacedObject placedObject;
			private readonly List<PlacedObject> otherPlaces;

			public CuriousObject(PlacedObject placedObject, Room room)
			{
				this.placedObject = placedObject;
				this.room = room;
				otherPlaces = new List<PlacedObject>();

				// Finds aditional info from other objects
				foreach (PlacedObject pobj in room.roomSettings.placedObjects)
				{
					if (pobj.type == EnumExt_ManagedPlacedObjects.CuriousObjectLocation && pobj.active)
						otherPlaces.Add(pobj);
				}

				LogMessage("CuriousObject started and found " + otherPlaces.Count + " location");
			}
			// IDrawable stuff
			public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
			{
				if (newContatiner == null) newContatiner = rCam.ReturnFContainer("Midground");
				for (int i = 0; i < sLeaser.sprites.Length; i++)
				{
					newContatiner.AddChild(sLeaser.sprites[i]);
				}
			}

			public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
			{
				for (int i = 0; i < sLeaser.sprites.Length; i++)
				{
					sLeaser.sprites[i].color = PlayerGraphics.SlugcatColor(new[] { SlugcatStats.Name.Yellow, SlugcatStats.Name.White, SlugcatStats.Name.Red, SlugcatStats.Name.Night }[UnityEngine.Random.Range(0, 4)]);
				}
			}

			public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
			{
				for (int i = 0; i < sLeaser.sprites.Length; i++)
				{
					sLeaser.sprites[i].SetPosition(otherPlaces[i].pos + ((CuriousData)this.placedObject.data).extraPos - camPos);
					sLeaser.sprites[i].scale = ((CuriousData)this.placedObject.data).GetValue<float>("scale");
					sLeaser.sprites[i].rotation = ((CuriousData)this.placedObject.data).rotation;
					Color clr = sLeaser.sprites[i].color;
					clr.r = ((CuriousData)this.placedObject.data).redcolor;
					sLeaser.sprites[i].color = clr;
				}
			}

			public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
			{
				sLeaser.sprites = new FSprite[otherPlaces.Count];
				for (int i = 0; i < sLeaser.sprites.Length; i++)
				{
					sLeaser.sprites[i] = new FSprite("HeadA0");
				}
				this.AddToContainer(sLeaser, rCam, null);
			}
		}

		// The data for our curious object
		// We declare a managed field called "scale" in the base constructor,
		// and another one called "red" that is tied to an actual field that can be accessed directly
		// some more managed fields that arent used for anything
		// and we create another one called rotation that we manage on our own
		class CuriousData : ManagedData
		{
#pragma warning disable 0649 // We're reflecting over these fields, stop worrying about it stupid compiler
			// A field can be generated like this, and its value can be accessed directly
			[FloatField("red", 0f, 1f, 0f, displayName: "Red Color")]
			public float redcolor;

			[StringField("mystring", "aaaaaaaa", "My String")]
			public string mystring = "aaaaaaaa";

			// For certain types it's not possible to use the Attribute notation, and you'll have to pass the field to the constructor
			// but you can still link a field in your object to the managed field and they will stay in sync.
			[BackedByField("ev2")]
			public Vector2 extraPos;

			// Just make sure you pass all the expected fields to the ManagedData contructor
			[BackedByField("ev3")]
			public Vector2 extraPos2;

			// Until there is a better implementation, you'll have to do this for Vector2Field, IntVector2Field, EnumField and ColorField.
			[BackedByField("msid")]
			public SoundID mySound = SoundID.Bat_Afraid_Flying_Sounds;
#pragma warning restore 0649

			private static ManagedField[] customFields = new ManagedField[] {
					new FloatField("scale", 0.1f, 10f, 1f, displayName:"Scale"),
					new Vector2Field("ev2", new Vector2(-100, -40), Vector2Field.VectorReprType.line),
                    //new Vector2Field("ev3", new Vector2(-100, -40), Vector2Field.VectorReprType.none),
                    new DrivenVector2Field("ev3", "ev2", new Vector2(-100, -40)), // Combines two vector2s in one single constrained control
                    new ExtEnumField<SoundID>("msid", SoundID.Bat_Afraid_Flying_Sounds, new SoundID[]{SoundID.Bat_Afraid_Flying_Sounds, SoundID.Bat_Attatch_To_Chain}, displayName:"What sound a bat makes"),
				};

			// that one field we didn't want to use the framework for, for whatever reason
			public float rotation;

			public CuriousData(PlacedObject owner) : base(owner, customFields)
			{
				this.rotation = UnityEngine.Random.value * 360f;
			}

			// Serialization has to include our manual field
			public override string ToString()
			{
				//Debug.Log("CuriousData serializing as " + base.ToString() + "~" + rotation);
				return base.ToString() + "~" + rotation;
			}

			public override void FromString(string s)
			{
				//Debug.Log("CuriousData deserializing from "+ s);
				base.FromString(s);
				string[] arr = Regex.Split(s, "~");
				try
				{
					rotation = float.Parse(arr[base.FieldsWhenSerialized + 0]);
				}
				catch { } // bad data, hopefully the default is fine :)
						  //Debug.Log("CuriousData got rotation = " + rotation);
			}
		}

		// Representation... ManagedData takes care of creating controls for managed fields
		// but we have one unmanaged field to control
		class CuriousRepresentation : ManagedRepresentation
		{
			public CuriousRepresentation(PlacedObject.Type placedType, ObjectsPage objPage, PlacedObject pObj) : base(placedType, objPage, pObj) { }

			public override void Update()
			{
				base.Update();
				if (UnityEngine.Input.GetKey("b")) return;
				(pObj.data as CuriousData)!.rotation = RWCustom.Custom.VecToDeg(this.owner.mousePos - absPos);
			}
		}
	}
	// And that was it, 3 somewhat functional objects in not an awful lot of code.
}
#pragma warning restore 1591
