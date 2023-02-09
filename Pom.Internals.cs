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

	private static readonly List<ManagedObjectType> managedObjectTypes = new List<ManagedObjectType>();
	/// <summary>
	/// Called from the hooks, finds the manager for the type, if any.
	/// </summary>
	private static ManagedObjectType? GetManagerForType(PlacedObject.Type tp)
	{
		foreach (var manager in managedObjectTypes)
		{
			if (tp == manager.GetObjectType() && tp != PlacedObject.Type.None) return manager;
		}
		return null;
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
