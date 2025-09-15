﻿namespace Pom;

public static partial class Pom
{

	private static readonly List<ManagedObjectType> managedObjectTypes = new List<ManagedObjectType>();
	/// <summary>
	/// Called from the hooks, finds the manager for the type, if any.
	/// </summary>
	private static ManagedObjectType? GetManagerForType(PlacedObject.Type tp)
	{
		CullUnregisteredTypes();
		foreach (var manager in managedObjectTypes)
		{
			if (tp == manager.GetObjectType() && tp != PlacedObject.Type.None) return manager;
		}
		return null;
	}

	private static void RemoveManagersForType(PlacedObject.Type tp)
	{
		managedObjectTypes.RemoveAll(x => x.GetObjectType() == tp);
	}

	private static void CullUnregisteredTypes()
	{
		managedObjectTypes.RemoveAll(x => x.GetObjectType().Index == -1);
	}

	private static PlacedObject.Type DeclareOrGetEnum(string name)
	{
		// enum handling is delayed because of how enumextend works
		// bee needs to let mods add enums during onload or this will be always super annoying to use
		if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name is empty");
		// nope, this crashes because enumextender hasnt readied its pants yet
		//if (PastebinMachine.EnumExtender.EnumExtender.declarations.Count > 0) PastebinMachine.EnumExtender.EnumExtender.ExtendEnumsAgain();
		PlacedObject.Type tp;

		if (PlacedObject.Type.TryParse(typeof(PlacedObject.Type), name, false, out ExtEnumBase o_tp))
		{
			tp = (PlacedObject.Type)o_tp;
		}
		else
		{
			tp = new PlacedObject.Type(name, true);
		}

		return tp;
	}
}
