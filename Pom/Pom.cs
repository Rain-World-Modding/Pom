namespace Pom;

/// <summary>
/// Functionality for registering placed object with rich devtools controls
/// </summary>
public static partial class Pom
{
	/// <summary>
	/// Register a <see cref="ManagedObjectType"/> or <see cref="FullyManagedObjectType"/> to handle object, data and repr initialization during room load and devtools hooks
	/// </summary>
	/// <param name="obj">ManagedObjectType you have constructed to define the object</param>
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
	/// <param name="managedFields">An array containing all fields the object should have</param>
	/// <param name="type">An UpdateableAndDeletable</param>
	/// <param name="name">Optional enum-name for your object, otherwise infered from type. Can be an enum already created with Enumextend. Do NOT use enum.ToString() on an enumextend'd enum, it wont work during Init() or Load()</param>
	/// <param name="category">Category of the object (null if unsorted)</param>
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
	/// <param name="name">Name of the object</param>
	/// <param name="category">Category of the object (null if unsorted)</param>
	/// <param name="dataType">Type of ManagedData (usually your child class)</param>
	/// <param name="reprType">Type of ManagedRepresentation (usually <see cref="global::Pom.Pom.ManagedRepresentation"/>)</param>
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
	/// Same as <see cref="RegisterEmptyObjectType"/>, but with generics.
	/// </summary>
	/// <typeparam name="DATA">Type of ManagedData (usually your child class)</typeparam>
	/// <typeparam name="REPR">Type of ManagedRepresentation (usually <see cref="global::Pom.Pom.ManagedRepresentation"/>)</typeparam>
	/// <param name="key">Name of the object</param>
	/// <param name="category">Category of the object (null if unsorted)</param>
	public static void RegisterEmptyObjectType<DATA, REPR>(string key, string? category = null)
		where DATA : ManagedData
		where REPR : ManagedRepresentation
	{
		RegisterEmptyObjectType(key, category, typeof(DATA), typeof(REPR));
	}
	/// <summary>
	/// Same as <see cref="RegisterManagedObject(ManagedObjectType)"/>, but with generics.
	/// </summary>
	/// <typeparam name="UAD">Type of UpdatableAndDeletable</typeparam>
	/// <typeparam name="DATA">Type of ManagedData (usually your child class)</typeparam>
	/// <typeparam name="REPR">Type of ManagedRepresentation (usually <see cref="global::Pom.Pom.ManagedRepresentation"/>)</typeparam>
	/// <param name="key">Name of the object</param>
	/// <param name="category">Category of the object (null if unsorted)</param>
	/// <param name="singleInstance">Whether only one object is allowed per room</param>
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
	/// <summary>
	/// Forces a selected PlacedObjectType to be sorted into specific category. Works for non-POM objects. <para/>
	/// Note that calling this force-creates the category.
	/// </summary>
	public static void RegisterCategoryOverride(PlacedObject.Type type, string category)
	{
		if (type.Index < 0)
		{
			LogWarning($"Tried creating a category override for {type} but it is not registered! skipping");
			return;
		}
		__objectCategories.Add(type.value, new(category, true));
	}
	/// <summary>
	/// Selects how an object category should be sorted.<para/>
	/// Note that calling this force-creates the category.
	/// </summary>
	/// <param name="category"></param>
	/// <param name="sortBehavior"></param>
	public static void RegisterSortingOverride(string category, CategorySortKind sortBehavior)
	{
		__sortCategorySettings.Add(new(category, true), sortBehavior);
		//if (new Category)
	}
}
