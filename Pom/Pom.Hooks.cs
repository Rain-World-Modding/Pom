using DevInterface;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

using ObjCategory = DevInterface.ObjectsPage.DevObjectCategories;

namespace Pom;

public static partial class Pom
{
	/// <summary>
	/// How to sort object entries inside a devtools object category
	/// </summary>
	public enum CategorySortKind
	{
		/// <summary>
		/// No sorting, items in the order they were registered
		/// </summary>
		Default,
		/// <summary>
		/// Alphabetical sorting (invariant culture)
		/// </summary>
		Alphabetical,
	}
	private static bool __hooked = false;
	//internal static ObjectsPage.DevObjectCategories __pomcat = new("POM", true);
	internal static Dictionary<string, ObjCategory> __objectCategories = new();
	internal static Dictionary<ObjCategory, CategorySortKind> __sortCategorySettings = new();
	internal static CategorySortKind __sortByDefault = CategorySortKind.Default;
	/// <summary>
	/// Applies the necessary hooks for the framework to do its thing.
	/// Called when any managed object is registered.
	/// </summary>
	private static void Apply()
	{
		if (__hooked) return;
		__hooked = true;
		On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_Sort;
		On.DevInterface.PositionedDevUINode.Move += OnPositionedDevUINodeMove;
		On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData_Patch;
		On.Room.Loaded += Room_Loaded_Patch;
		On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep_Patch;
		try
		{
			ApplyInputHooks();
		}
		catch (Exception ex)
		{
			LogError($"Error adding input hooks {ex}");
		}
		try
		{
			IL.DevInterface.ObjectsPage.AssembleObjectPages += IL_ObjectsPage_AssemblePages;
		}
		catch (Exception ex)
		{
			LogError($"Error adding sorter ilhook {ex}");
		}
		//PlacedObjectsExample();
	}
	private static void IL_ObjectsPage_AssemblePages(ILContext il)
	{
		ILCursor c = new(il);
		c.GotoNext(MoveType.After, x => x.MatchStloc(1));
		c.Emit(OpCodes.Ldloc_0);
		c.EmitDelegate((Dictionary<ObjCategory, List<PlacedObject.Type>> dict) =>
		{
			LogDebug("Sorter ilhook go");
			foreach (var kvp in dict)
			{
				var cat = kvp.Key;
				var list = kvp.Value;
				if (!__sortCategorySettings.TryGetValue(cat, out CategorySortKind doSort)) doSort = __sortByDefault;
				if (doSort is CategorySortKind.Default)
				{
					LogDebug($"Sorting of {cat} not required");
					continue;
				}
				System.Comparison<PlacedObject.Type> sorter = doSort switch
				{
					CategorySortKind.Alphabetical => static (ot1, ot2) =>
					{
						if (ot1 == PlacedObject.Type.None && ot2 == PlacedObject.Type.None) return 0;
						if (ot1 == PlacedObject.Type.None) return 1;
						if (ot2 == PlacedObject.Type.None) return -1;
						else return StringComparer.InvariantCulture.Compare(ot1?.value, ot2?.value);
					}
					,
					_ => throw new ArgumentException($"ERROR: INVALID {nameof(CategorySortKind)} VALUE {doSort}")
				};
				list.Sort(sorter);
				LogDebug($"sorting of {cat} completed ({list.Count} items)");
			}
		});
	}

	private static ObjCategory ObjectsPage_Sort(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, DevInterface.ObjectsPage self, PlacedObject.Type type)
	{
		if (__objectCategories.TryGetValue(type.value, out ObjectsPage.DevObjectCategories cat))
		{
			LogDebug($"Sorting {type} into {cat}");
			return cat;
		}
		return orig(self, type);
	}

	private static void ObjectsPage_CreateObjRep_Patch(
		On.DevInterface.ObjectsPage.orig_CreateObjRep orig,
		ObjectsPage self,
		PlacedObject.Type tp,
		PlacedObject pObj)
	{
		orig(self, tp, pObj);
		if (GetManagerForType(tp) is not ManagedObjectType manager) return;

		var isNew = pObj == null;
		if (isNew) pObj = self.RoomSettings.placedObjects[self.RoomSettings.placedObjects.Count - 1];
		DevInterface.PlacedObjectRepresentation? placedObjectRepresentation = manager.MakeRepresentation(pObj, self);
		if (placedObjectRepresentation == null) return;

		DevInterface.PlacedObjectRepresentation old = (DevInterface.PlacedObjectRepresentation)self.tempNodes.Pop();
		self.subNodes.Pop();
		old.ClearSprites();
		self.tempNodes.Add(placedObjectRepresentation);
		self.subNodes.Add(placedObjectRepresentation);

		if (isNew)
		{
			try
			{
				self.owner.room.abstractRoom.firstTimeRealized = true;
				var obj = manager.MakeObject(pObj, self.owner.room);
				if (obj != null)
				{
					self.owner.room.AddObject(obj);
				}
			}
			finally
			{
				self.owner.room.abstractRoom.firstTimeRealized = false;
			}
		}
	}

	private static void Room_Loaded_Patch(
		On.Room.orig_Loaded orig,
		Room self)
	{
		var firstTimeRealized = self.abstractRoom.firstTimeRealized;
		orig(self);
		if (self.game is null) return;
		try
		{
			self.abstractRoom.firstTimeRealized = firstTimeRealized;
			for (int i = 0; i < self.roomSettings.placedObjects.Count; i++)
			{
				if (self.roomSettings.placedObjects[i].active)
				{
					if (GetManagerForType(self.roomSettings.placedObjects[i].type) is ManagedObjectType man)
					{
						UpdatableAndDeletable? obj = man.MakeObject(self.roomSettings.placedObjects[i], self);
						if (obj == null) continue;
						self.AddObject(obj);
					}
				}
			}
		}
		finally
		{
			self.abstractRoom.firstTimeRealized = false;
		}
	}

	private static void PlacedObject_GenerateEmptyData_Patch(
		On.PlacedObject.orig_GenerateEmptyData orig,
		PlacedObject self)
	{
		orig(self);
		if (GetManagerForType(self.type) is ManagedObjectType manager)
		{
			self.data = manager.MakeEmptyData(self);
		}
	}

	internal static void OnPositionedDevUINodeMove(
				On.DevInterface.PositionedDevUINode.orig_Move orig,
				PositionedDevUINode self,
				Vector2 newpos)
	{
		orig(self, newpos);
		if (self.parentNode is Vector2ArrayField.Vector2ArrayHandle v2ah)
		{
			Vector2[] vectors = v2ah.Data.GetValue<Vector2[]>(v2ah.Field.key)!;
			int index = int.Parse(self.IDstring.Split('_')[1]);
			if (index == 0) return;
			vectors[index] = newpos;
			v2ah.Data.SetValue(v2ah.Field.key, vectors);
		}
	}
}
