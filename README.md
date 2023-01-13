# Pom

Port of henpemaz' mighty PlacedObjectsManager to Downpour, with some additional ergonomics (such as attribute-usable enumfields).

## Usage

To add a new placed object, follow these steps:

- Create a class deriving from `Pom.Pom.ManagedData`. This class will hold all the automatically serialized data that will be attached to your object and changed when you click your object's handles and buttons in devinterface.
	- Each field you intend to use as a serialized data field, you denote with an appropriate `ManagedField` attribute (they are all nested types inside Pom.Pom). Float fields are denoted with `FloatField`, ExtEnum fields are with `ExtEnumField` etc.
- [optional] Create a class deriving from UpdatableAndDeletable, with a constructor of one of the following forms: `PlacedObject, Room`, `Room, PlacedObject`, `Room`. This will be instantiated in rooms where your placedobject is added. `data` of the `PlacedObject` passed to the constructor will always be of your type in previous step, you can cast to it to use that data. 
- [optional] Create a class deriving from `Pom.Pom.ManagedRepresentation`. Doing this allows you to (in theory) create your own custom visual representation in devUI, but is hardly ever needed. Use Pom.Pom.ManagedRepresentation by default.
- Use one of the following static functions found in `Pom.Pom` to register your object:

| Function	| Desctiption	|
| ---		| ---			|
| `RegisterManagedObject` `<UAD,DATA,REPR>` `(string key, bool singleInstance)` | Register a full object. Generic parameters are: your UpdatableAndDeletable type,  your `ManagedData` type, and your representation type (Pom.Pom.ManagedRepresentation for default repr). `key` is the name of your object, set `singleInstance` to `true` if you want only one of your `UAD` to be created per room. |
| `RegisterEmptyObjectType` `<DATA,REPR>` `(string key)` | Register an "empty" object that will not create an UpdatableAndDeletable. Generic parameters are:  your `ManagedData` type and your representation type (Pom.Pom.ManagedRepresentation for default repr). `key` is the name of your object. |

You're done! Your object will now appear in POM category of devtools objects tab.
