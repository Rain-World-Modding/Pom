using DevInterface;
using MonoMod.Cil;
using Mono.Cecil.Cil;

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
		On.DevInterface.PositionedDevUINode.Move += Vector2ArrayField.OnPositionedDevUINodeMove;
		On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData_Patch;
		On.Room.Loaded += Room_Loaded_Patch;
		On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep_Patch;
		try
		{
			IL.DevInterface.ObjectsPage.AssembleObjectPages += IL_ObjectsPage_AssemblePages;
		}
		catch (Exception ex)
		{
			plog.LogError($"Error adding sorter ilhook {ex}");
		}
		//PlacedObjectsExample();
	}
	private static void IL_ObjectsPage_AssemblePages(MonoMod.Cil.ILContext il)
	{
		plog.LogDebug("ILHook body start");
		MonoMod.Cil.ILCursor c = new(il);
		c.GotoNext(MonoMod.Cil.MoveType.Before,
			//x => x.MatchLdcI4(0),
			x => x.MatchStloc(2),
			x => x.MatchLdarg(0),
			x => x.MatchLdloc(1),
			x => x.MatchNewarr<PlacedObject.Type>()
			);//TryFindNext(out )
		plog.LogDebug($"Found inj point, emitting");
		//c.Remove();
		c.Emit(OpCodes.Pop);
		c.Emit(OpCodes.Ldloc_0);
		//c.Index -= 1;
		//var newLabel = c.MarkLabel();
		//c.Index += 1;
		c.EmitDelegate((Dictionary<ObjCategory, List<PlacedObject.Type>> dict) =>
		{
			plog.LogDebug("Sorter ilhook go");
			foreach (var kvp in dict)
			{
				var cat = kvp.Key;
				var list = kvp.Value;
				if (!__sortCategorySettings.TryGetValue(cat, out CategorySortKind doSort)) doSort = __sortByDefault;
				if (doSort is CategorySortKind.Default)
				{
					plog.LogDebug($"Sorting of {cat} not required");
					continue;
				}
				System.Comparison<PlacedObject.Type> sorter = doSort switch
				{
					CategorySortKind.Alphabetical => static (ot1, ot2) => {
						if (ot1 == PlacedObject.Type.None && ot2 == PlacedObject.Type.None) return 0;
						if (ot1 == PlacedObject.Type.None) return 1;
						if (ot2 == PlacedObject.Type.None) return -1;
						else return StringComparer.InvariantCulture.Compare(ot1?.value, ot2?.value);},
					_ => throw new ArgumentException($"ERROR: INVALID {nameof(CategorySortKind)} VALUE {doSort}")
				};
				list.Sort(sorter);
				plog.LogDebug($"sorting of {cat} completed ({list.Count} items)");
			}
		});
		c.Emit(OpCodes.Ldc_I4_0);
		plog.LogDebug("emit complete");
		plog.LogDebug(il.ToString());
	}

	private static ObjCategory ObjectsPage_Sort(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, DevInterface.ObjectsPage self, PlacedObject.Type type)
	{
		if (__objectCategories.TryGetValue(type.value, out ObjectsPage.DevObjectCategories cat))
		{
			plog.LogDebug($"Sorting {type} into {cat}");
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

		if (pObj == null) pObj = self.RoomSettings.placedObjects[self.RoomSettings.placedObjects.Count - 1];
		DevInterface.PlacedObjectRepresentation? placedObjectRepresentation = manager.MakeRepresentation(pObj, self);
		if (placedObjectRepresentation == null) return;

		DevInterface.PlacedObjectRepresentation old = (DevInterface.PlacedObjectRepresentation)self.tempNodes.Pop();
		self.subNodes.Pop();
		old.ClearSprites();
		self.tempNodes.Add(placedObjectRepresentation);
		self.subNodes.Add(placedObjectRepresentation);
	}

	private static void Room_Loaded_Patch(
		On.Room.orig_Loaded orig,
		Room self)
	{
		orig(self);
		if (self.game is null) return;
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

}
