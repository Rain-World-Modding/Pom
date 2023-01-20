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

	/// <summary>
	/// Register a <see cref="ManagedObjectType"/> or <see cref="FullyManagedObjectType"/> to handle object, data and repr initialization during room load and devtools hooks
	/// </summary>
	/// <param name="obj"></param>
	public static void RegisterManagedObject(ManagedObjectType obj)
	{
		Apply();
		//new PlacedObject.Type(obj)
		managedObjectTypes.Add(obj);
	}
	/// <summary>
	/// Shorthand for registering a <see cref="FullyManagedObjectType"/>.
	/// Wraps an UpdateableAndDeletable into a managed type with managed data and UI.
	/// Can also be used with a null type to spawn a Managed data+representation with no object on room.load
	/// If the object isn't null its Constructor should take (Room, PlacedObject) or (PlacedObject, Room).
	/// </summary>
	/// <param name="managedFields"></param>
	/// <param name="type">An UpdateableAndDeletable</param>
	/// <param name="name">Optional enum-name for your object, otherwise infered from type. Can be an enum already created with Enumextend. Do NOT use enum.ToString() on an enumextend'd enum, it wont work during Init() or Load()</param>
	public static void RegisterFullyManagedObjectType(
		ManagedField[] managedFields,
		Type type,
		string? name = null,
		string? category = null)
	{
		if (string.IsNullOrEmpty(name)) name = type.Name;
		ManagedObjectType fullyManaged = new FullyManagedObjectType(name, category, type, managedFields);
		RegisterManagedObject(fullyManaged);
	}

	/// <summary>
	/// Shorthand for registering a ManagedObjectType with no actual object.
	/// Creates an empty data-holding placed object.
	/// Data and Repr must work well together (typically rep tries to cast data to a specific type to use it).
	/// Either can be left null, so no data or no specific representation will be created for the placedobject.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="dataType"></param>
	/// <param name="reprType"></param>
	public static void RegisterEmptyObjectType(
		string name,
		string? category,
		Type dataType,
		Type reprType)
	{
		ManagedObjectType emptyObjectType = new ManagedObjectType(name, category ?? "POM", null, dataType, reprType);
		RegisterManagedObject(emptyObjectType);
	}
	/// <summary>
	/// Same as <see cref="RegisterEmptyObjectType(string, Type, Type)"/>, but with generics.
	/// </summary>
	/// <typeparam name="DATA"></typeparam>
	/// <typeparam name="REPR"></typeparam>
	/// <param name="key"></param>
	public static void RegisterEmptyObjectType<DATA, REPR>(string key, string? category = null)
		where DATA : ManagedData
		where REPR : ManagedRepresentation
	{
		RegisterEmptyObjectType(key, category, typeof(DATA), typeof(REPR));
	}
	/// <summary>
	/// Same as <see cref="RegisterManagedObject(ManagedObjectType)"/>, but with generics.
	/// </summary>
	/// <typeparam name="UAD"></typeparam>
	/// <typeparam name="DATA"></typeparam>
	/// <typeparam name="REPR"></typeparam>
	/// <param name="key"></param>
	/// <param name="singleInstance"></param>
	public static void RegisterManagedObject<UAD, DATA, REPR>(
		string key,
		string? category = null,
		bool singleInstance = false)
		where UAD : UpdatableAndDeletable
		where DATA : ManagedData
		where REPR : ManagedRepresentation
	{
		RegisterManagedObject(new ManagedObjectType(key, category ?? "POM", typeof(UAD), typeof(DATA), typeof(REPR), singleInstance));
	}
}
