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
	private static bool __hooked = false;
	//internal static ObjectsPage.DevObjectCategories __pomcat = new("POM", true);
	internal static Dictionary<string, ObjectsPage.DevObjectCategories> __categories = new();
	//private readonly static List<string> __moddedTypes = new();
	//private static int __unmodVer = false;
	/// <summary>
	/// Applies the necessary hooks for the framework to do its thing.
	/// Called when any managed object is registered.
	/// </summary>
	private static void Apply()
	{
		if (__hooked) return;
		__hooked = true;
		On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += (orig, self, type) =>
		{
			if (__categories.TryGetValue(type.value, out ObjectsPage.DevObjectCategories cat)){
				plog.LogDebug($"Sorting {type} into {cat}");
				return cat;
			}
			return orig(self, type);
		};
		On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData_Patch;
		On.Room.Loaded += Room_Loaded_Patch;
		On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep_Patch;

		//PlacedObjectsExample();
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
